using TMPro;
using UnityEngine;

// HUD da fase: cronômetro, contador de filhotes e a tela de resultado.
// Fica em um objeto do Canvas. Todos os campos de texto são opcionais.
public class LevelHUD : MonoBehaviour {
    [SerializeField] LevelObjective objective;

    [Header("Durante a fase")]
    [SerializeField] TMP_Text timerText;
    [SerializeField] TMP_Text ducklingsText;
    [Tooltip("{0} = filhotes com a mamãe, {1} = total.")]
    [SerializeField] string ducklingsFormat = "Filhotes: {0}/{1}";

    [Header("Fase completada")]
    [Tooltip("Painel que aparece ao vencer (começa desligado). Opcional: deixe vazio se a tela de vitória for de outro sistema.")]
    [SerializeField] GameObject resultPanel;
    [SerializeField] TMP_Text resultTimeText;
    [Tooltip("{0} = tempo final no formato mm:ss.cc")]
    [SerializeField] string resultFormat = "Tempo: {0}";

    void Awake() {
        if (objective == null) objective = FindFirstObjectByType<LevelObjective>();
        if (resultPanel != null) resultPanel.SetActive(false);
    }

    void OnEnable() {
        if (objective != null) objective.LevelCompleted += OnLevelCompleted;
    }

    void OnDisable() {
        if (objective != null) objective.LevelCompleted -= OnLevelCompleted;
    }

    void Update() {
        if (objective == null) return;

        if (timerText != null) timerText.text = LevelObjective.FormatTime(objective.ElapsedTime);
        if (ducklingsText != null)
            ducklingsText.text = string.Format(ducklingsFormat, objective.ProgressCount, objective.TotalDucklings);
    }

    void OnLevelCompleted(float time) {
        if (resultTimeText != null) resultTimeText.text = string.Format(resultFormat, LevelObjective.FormatTime(time));
        if (resultPanel != null) resultPanel.SetActive(true);
    }
}