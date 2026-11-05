using System.Collections.Generic;
using UnityEngine;

public class DucklingIndicatorManager : MonoBehaviour {
    [Header("Referências")]
    [Tooltip("O DuckQuack do Player. Se ficar vazio, procura na cena.")]
    [SerializeField] DuckQuack quack;
    [Tooltip("A câmera do jogo. Se ficar vazio, usa a Main Camera.")]
    [SerializeField] Camera cam;
    [Tooltip("O Canvas onde as bolhas aparecem. Se ficar vazio, usa o Canvas deste objeto.")]
    [SerializeField] Canvas canvas;
    [Tooltip("Prefab da bolha (com DucklingIndicator, âncora no centro).")]
    [SerializeField] DucklingIndicator indicatorPrefab;
    [Tooltip("Onde as bolhas são criadas. Deve ocupar o Canvas inteiro, com âncora no centro. Se ficar vazio, usa o Canvas.")]
    [SerializeField] RectTransform container;

    [Header("Posição")]
    [Tooltip("Distância (pixels) da borda da tela em que a bolha para quando o filhote está fora da tela.")]
    [SerializeField] float edgeMargin = 80f;
    [Tooltip("Altura (m) acima do filhote em que a bolha aparece quando ele está na tela.")]
    [SerializeField] float worldOffsetY = 0.9f;
    [Tooltip("Distância (unidades do Canvas) da seta até o centro da bolha.")]
    [SerializeField] float arrowDistance = 55f;

    [Header("Tempo")]
    [Tooltip("Quanto tempo (s) a bolha fica na tela depois do Quack.")]
    [SerializeField] float showDuration = 2.5f;
    [Tooltip("Duração (s) do aparecer e do sumir.")]
    [SerializeField] float fadeTime = 0.25f;
    [Tooltip("Tamanho extra da bolha no instante em que aparece (efeito de 'pulo').")]
    [SerializeField] float popScale = 1.3f;

    class Entry {
        public Duckling duckling;
        public DucklingIndicator view;
        public float timeLeft;
        public float age;
    }

    readonly Dictionary<Duckling, Entry> entries = new Dictionary<Duckling, Entry>();
    readonly List<Duckling> toRemove = new List<Duckling>();

    void Awake() {
        if (canvas == null) canvas = GetComponentInParent<Canvas>();
        if (container == null && canvas != null) container = (RectTransform)canvas.transform;
        if (cam == null) cam = Camera.main;
    }

    void OnEnable() => Subscribe();

    void Start() {
        // O Player pode ter sido criado depois deste objeto
        if (quack == null) Subscribe();
        if (cam == null) cam = Camera.main;
    }

    void OnDisable() {
        if (quack != null) quack.DucklingResponded -= OnDucklingResponded;
        ClearAll();
    }

    void Subscribe() {
        if (quack == null) quack = FindFirstObjectByType<DuckQuack>();
        if (quack == null) return;
        quack.DucklingResponded -= OnDucklingResponded;   // evita assinar duas vezes
        quack.DucklingResponded += OnDucklingResponded;
    }

    void OnDucklingResponded(Duckling duckling, Vector2 direction) {
        if (indicatorPrefab == null || container == null) return;

        if (!entries.TryGetValue(duckling, out Entry entry)) {
            entry = new Entry { duckling = duckling, view = Instantiate(indicatorPrefab, container) };
            entries.Add(duckling, entry);
        }

        // Novo Quack: renova o tempo e repete o "pulo"
        entry.timeLeft = showDuration;
        entry.age = 0f;
        entry.view.SetVisible(true);
    }

    void LateUpdate() {
        if (entries.Count == 0 || cam == null || quack == null) return;

        Vector2 motherScreen = cam.WorldToScreenPoint(quack.transform.position);
        Rect safe = new Rect(edgeMargin, edgeMargin, Screen.width - 2f * edgeMargin, Screen.height - 2f * edgeMargin);

        toRemove.Clear();

        foreach (var pair in entries) {
            Entry e = pair.Value;
            Duckling d = e.duckling;

            // Sumiu, foi recuperado ou o tempo acabou
            if (d == null || !d.isActiveAndEnabled || d.IsRecovered || e.timeLeft <= 0f) {
                toRemove.Add(pair.Key);
                continue;
            }

            e.timeLeft -= Time.deltaTime;
            e.age += Time.deltaTime;

            Vector2 targetScreen = cam.WorldToScreenPoint(d.transform.position + Vector3.up * worldOffsetY);
            bool onScreen = safe.Contains(targetScreen);

            Vector2 screenPos;
            Vector2 arrowDir = Vector2.zero;

            if (onScreen) {
                screenPos = targetScreen;
            }
            else {
                // A linha da mamãe até o filhote cruza a margem da tela: é onde a bolha fica
                screenPos = EdgePoint(motherScreen, targetScreen, safe);
                arrowDir = targetScreen - motherScreen;
            }

            float fadeIn = Mathf.Clamp01(e.age / fadeTime);
            float fadeOut = Mathf.Clamp01(e.timeLeft / fadeTime);
            float alpha = Mathf.Min(fadeIn, fadeOut);
            float scale = Mathf.Lerp(popScale, 1f, Mathf.Clamp01(e.age / 0.2f));

            e.view.Place(ToCanvas(screenPos), scale, alpha, arrowDir, arrowDistance);
        }

        foreach (Duckling key in toRemove) Remove(key);
    }
    
    static Vector2 EdgePoint(Vector2 from, Vector2 target, Rect rect) {
        from.x = Mathf.Clamp(from.x, rect.xMin, rect.xMax);
        from.y = Mathf.Clamp(from.y, rect.yMin, rect.yMax);

        Vector2 d = target - from;
        float t = float.MaxValue;

        if (d.x > 0.0001f) t = Mathf.Min(t, (rect.xMax - from.x) / d.x);
        else if (d.x < -0.0001f) t = Mathf.Min(t, (rect.xMin - from.x) / d.x);

        if (d.y > 0.0001f) t = Mathf.Min(t, (rect.yMax - from.y) / d.y);
        else if (d.y < -0.0001f) t = Mathf.Min(t, (rect.yMin - from.y) / d.y);

        if (t == float.MaxValue) return from;
        return from + d * Mathf.Min(t, 1f);
    }

    Vector2 ToCanvas(Vector2 screenPoint) {
        Camera uiCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)canvas.transform, screenPoint, uiCam, out Vector2 local);
        return local;
    }

    void Remove(Duckling key) {
        if (entries.TryGetValue(key, out Entry e)) {
            if (e.view != null) Destroy(e.view.gameObject);
            entries.Remove(key);
        }
    }

    void ClearAll() {
        foreach (var pair in entries)
            if (pair.Value.view != null) Destroy(pair.Value.view.gameObject);
        entries.Clear();
    }
}