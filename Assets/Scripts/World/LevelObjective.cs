using System;
using UnityEngine;

// Objetivo da fase: conta os filhotes, controla o cronômetro e decide quando a fase foi completada.
// Coloque UM por fase, em um objeto vazio (ex.: "LevelObjective" ou junto do LevelManager).
//
// Fluxo da vitória:
//   1) Todos os filhotes foram coletados e estão seguindo a mamãe  -> AllDucklingsGathered (começa a contagem)
//   2) Passam-se 'victoryDelay' segundos                           -> LevelCompleted (+ GameEvents.GameSystem.OnLevelCompleted)
//   Se um filhote se perder durante a contagem, ela cancela (CountdownCancelled), se 'cancelIfLost' estiver ligado.
public class LevelObjective : MonoBehaviour {
    public static LevelObjective Instance { get; private set; }

    [Header("Filhotes")]
    [Tooltip("Total de filhotes da fase. 0 = conta automaticamente os filhotes que existem na cena quando a fase começa.")]
    [SerializeField] int totalOverride = 0;

    [Header("Vitória")]
    [Tooltip("Tempo (segundos) entre cumprir a condição (todos os filhotes seguindo a mamãe) e a vitória ser ativada. " +
             "É o tempo que o fade out/transição tem para acontecer. 0 = vitória imediata.")]
    [SerializeField] float victoryDelay = 2f;
    [Tooltip("Se um filhote se perder durante a contagem, a contagem cancela e recomeça quando todos voltarem a seguir. " +
             "Desligado: depois que a contagem começa, a vitória acontece de qualquer jeito.")]
    [SerializeField] bool cancelIfLost = true;

    [Header("Cronômetro")]
    [Tooltip("Começa a contar assim que a fase carrega. Desligue para iniciar manualmente com StartTimer().")]
    [SerializeField] bool startTimerOnStart = true;

    public int TotalDucklings { get; private set; }
    public float ElapsedTime { get; private set; }
    public bool IsRunning { get; private set; }
    public bool IsCompleted { get; private set; }
    // Contagem da vitória em andamento (todos os filhotes já estão com a mamãe)
    public bool IsCountingDown { get; private set; }
    public float CountdownRemaining { get; private set; }

    // Filhotes já coletados alguma vez
    public int CollectedCount { get { int n = 0; foreach (var d in Duckling.All) if (d.IsRecovered) n++; return n; } }
    // Filhotes seguindo a mamãe agora
    public int FollowingCount { get { int n = 0; foreach (var d in Duckling.All) if (d.State == Duckling.DucklingState.Following) n++; return n; } }
    // Quantos estão com a mamãe agora. É o que a HUD mostra
    public int ProgressCount => FollowingCount;
    public int MissingCount => Mathf.Max(0, TotalDucklings - FollowingCount);

    // Todos os filhotes estão seguindo a mamãe: a contagem da vitória começou (bom momento para iniciar o fade)
    public event Action AllDucklingsGathered;
    // Um filhote se perdeu durante a contagem: ela foi cancelada (cancele o fade, se já tinha começado)
    public event Action CountdownCancelled;
    // A vitória foi ativada. Recebe o tempo final (segundos)
    public event Action<float> LevelCompleted;

    void Awake() {
        Instance = this;
    }

    void OnDestroy() {
        if (Instance == this) Instance = null;
    }

    void Start() {
        TotalDucklings = totalOverride > 0 ? totalOverride : Duckling.All.Count;
        if (TotalDucklings == 0)
            Debug.LogWarning("LevelObjective: nenhum filhote encontrado na cena. A vitória nunca será ativada.", this);

        if (startTimerOnStart) StartTimer();
    }

    void Update() {
        if (IsCompleted) return;

        // O tempo da fase fica parado durante a contagem da vitória
        if (IsRunning && !IsCountingDown) ElapsedTime += Time.deltaTime;

        bool met = ConditionMet();

        if (!IsCountingDown) {
            if (met) BeginCountdown();
            return;
        }

        if (cancelIfLost && !met) {
            CancelCountdown();
            return;
        }

        CountdownRemaining -= Time.deltaTime;
        if (CountdownRemaining <= 0f) Complete();
    }

    // Todos os filhotes coletados e seguindo a mamãe
    bool ConditionMet() => TotalDucklings > 0 && MissingCount == 0;

    void BeginCountdown() {
        IsCountingDown = true;
        CountdownRemaining = victoryDelay;
        AllDucklingsGathered?.Invoke();

        if (victoryDelay <= 0f) Complete();
    }

    void CancelCountdown() {
        IsCountingDown = false;
        CountdownRemaining = 0f;
        CountdownCancelled?.Invoke();
    }

    void Complete() {
        IsCompleted = true;
        IsCountingDown = false;
        IsRunning = false;
        LevelCompleted?.Invoke(ElapsedTime);
        GameEvents.GameSystem.OnLevelCompleted?.Invoke(ElapsedTime);
    }

    public void StartTimer() {
        if (IsCompleted) return;
        IsRunning = true;
    }

    public void PauseTimer() => IsRunning = false;

    // "mm:ss.cc"
    public static string FormatTime(float seconds) {
        int minutes = (int)(seconds / 60f);
        float rest = seconds - minutes * 60f;
        return $"{minutes:00}:{rest:00.00}";
    }
}