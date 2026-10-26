using System.Collections.Generic;
using UnityEngine;

public enum EyeMood { Default, Sad, Angry }

// Só visual: troca os olhos e a pose (em pé / caído) ligando e desligando os sprites do rig.
// Este script é o DONO desses objetos: não anime o "Active" deles em clips do Animator, senão os dois brigam.
public class DuckSpriteSwap : MonoBehaviour {
    [SerializeField] DuckMovement duck;

    [Header("Olhos (pose em pé)")]
    [SerializeField] GameObject eyesDefault;   // Olho
    [SerializeField] GameObject eyesSad;       // OlhoTriste
    [SerializeField] GameObject eyesAngry;     // OlhoBravo (ainda a definir quando usar)

    [Header("Poses")]
    [Tooltip("Partes do pato em pé, sem os olhos (Bico1, Cabeca, Corpo, PernaL...). Ficam escondidas na pose de queda.")]
    [SerializeField] GameObject[] standingParts;
    [Tooltip("Partes da pose de queda: as que terminam com C (OlhosC, CorpoC...). Só aparecem em Fallen.")]
    [SerializeField] GameObject[] fallenParts;

    [Header("Troca automática pelo estado do pato")]
    [SerializeField] bool autoFromState = true;
    [Tooltip("Olhos usados enquanto o pato se levanta (GettingUp).")]
    [SerializeField] EyeMood moodWhenGettingUp = EyeMood.Sad;

    public EyeMood Mood { get; private set; } = EyeMood.Default;
    public bool FallenPose { get; private set; }

    void Reset() => duck = GetComponentInParent<DuckMovement>();

    void Awake() {
        if (duck == null) duck = GetComponentInParent<DuckMovement>();
    }

    void OnEnable() {
        if (duck != null && autoFromState) duck.StateChanged += OnStateChanged;
        Apply();
    }

    void OnDisable() {
        if (duck != null) duck.StateChanged -= OnStateChanged;
    }

    void OnStateChanged(DuckState from, DuckState to) {
        switch (to) {
            case DuckState.Fallen:
                FallenPose = true;
                break;
            case DuckState.GettingUp:
                FallenPose = false;
                Mood = moodWhenGettingUp;
                break;
            default:
                FallenPose = false;
                Mood = EyeMood.Default;
                break;
        }
        Apply();
    }

    // Para controlar por script (outros sistemas, eventos, cutscenes)
    public void SetMood(EyeMood mood) { Mood = mood; Apply(); }
    public void SetFallenPose(bool fallen) { FallenPose = fallen; Apply(); }

    void Apply() {
        SetActive(standingParts, !FallenPose);
        SetActive(fallenParts, FallenPose);

        // Na pose de queda quem desenha os olhos é o OlhosC, então os olhos normais ficam escondidos
        bool eyes = !FallenPose;
        SetActive(eyesDefault, eyes && Mood == EyeMood.Default);
        SetActive(eyesSad, eyes && Mood == EyeMood.Sad);
        SetActive(eyesAngry, eyes && Mood == EyeMood.Angry);
    }

    static void SetActive(GameObject go, bool active) {
        if (go != null && go.activeSelf != active) go.SetActive(active);
    }

    static void SetActive(GameObject[] list, bool active) {
        if (list == null) return;
        foreach (var go in list) SetActive(go, active);
    }

#if UNITY_EDITOR
    // Botão direito no componente > "Preencher pelo nome dos objetos": monta as listas sozinho
    [ContextMenu("Preencher pelo nome dos objetos")]
    void AutoFill() {
        var standing = new List<GameObject>();
        var fallen = new List<GameObject>();

        foreach (var sr in GetComponentsInChildren<SpriteRenderer>(true)) {
            GameObject go = sr.gameObject;
            switch (go.name) {
                case "Olho": eyesDefault = go; continue;
                case "OlhoTriste": eyesSad = go; continue;
                case "OlhoBravo": eyesAngry = go; continue;
            }

            if (go.name.Length > 1 && go.name.EndsWith("C")) fallen.Add(go);
            else standing.Add(go);
        }

        standingParts = standing.ToArray();
        fallenParts = fallen.ToArray();
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}