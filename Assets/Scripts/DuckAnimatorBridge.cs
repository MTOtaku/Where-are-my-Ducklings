using System.Collections.Generic;
using UnityEngine;

public class DuckAnimatorBridge : MonoBehaviour {
    [SerializeField] DuckMovement duck;
    [Tooltip("O Animator do rig (no objeto raiz do rig, o que tem os ossos como filhos).")]
    [SerializeField] Animator animator;
    [SerializeField] GroundBalanceController ground;
    [Tooltip("O FlightController do Player: avisa a cada batida de asa para tocar a animação. Se ficar vazio, procura no Player.")]
    [SerializeField] FlightController flight;
    [SerializeField] MovementSettings movementSettings;

    [Header("Ciclo de andar")]
    [Tooltip("Velocidade do pato (m/s) em que o ciclo de andar NÃO patina no chão. Ajuste olhando o clip rodando no jogo.")]
    [SerializeField] float walkReferenceSpeed = 3f;
    [Tooltip("Limites do multiplicador de velocidade do ciclo de andar (mínimo, máximo).")]
    [SerializeField] Vector2 walkSpeedRange = new Vector2(0.6f, 2f);

    static readonly int StateId = Animator.StringToHash("State");
    static readonly int ModeId = Animator.StringToHash("Mode");
    static readonly int SpeedXId = Animator.StringToHash("SpeedX");
    static readonly int VelYId = Animator.StringToHash("VelY");
    static readonly int BalanceId = Animator.StringToHash("Balance");
    static readonly int FallenId = Animator.StringToHash("Fallen");
    static readonly int WalkSpeedId = Animator.StringToHash("WalkSpeed");
    static readonly int FlapLeftId = Animator.StringToHash("FlapLeft");
    static readonly int FlapRightId = Animator.StringToHash("FlapRight");

    // Só os parâmetros que EXISTEM no Controller (com o nome e o tipo certos) são enviados
    readonly HashSet<int> available = new HashSet<int>();

    void Awake() {
        if (flight == null && duck != null) flight = duck.GetComponent<FlightController>();

        if (animator == null || animator.runtimeAnimatorController == null) {
            Debug.LogWarning("DuckAnimatorBridge: o Animator está vazio ou sem Controller. Nada será enviado.", this);
            return;
        }

        var parameters = animator.parameters;
        var missing = new List<string>();

        Register(parameters, "State", AnimatorControllerParameterType.Int, missing);
        Register(parameters, "Mode", AnimatorControllerParameterType.Int, missing);
        Register(parameters, "SpeedX", AnimatorControllerParameterType.Float, missing);
        Register(parameters, "VelY", AnimatorControllerParameterType.Float, missing);
        Register(parameters, "Balance", AnimatorControllerParameterType.Float, missing);
        Register(parameters, "Fallen", AnimatorControllerParameterType.Bool, missing);
        Register(parameters, "WalkSpeed", AnimatorControllerParameterType.Float, missing);
        Register(parameters, "FlapLeft", AnimatorControllerParameterType.Trigger, missing);
        Register(parameters, "FlapRight", AnimatorControllerParameterType.Trigger, missing);

        if (missing.Count > 0)
            Debug.LogWarning("DuckAnimatorBridge: faltam estes parâmetros no Animator Controller (confira nome e tipo): "
                             + string.Join(", ", missing), this);
    }

    void Register(AnimatorControllerParameter[] parameters, string name, AnimatorControllerParameterType type, List<string> missing) {
        foreach (var p in parameters) {
            if (p.name == name && p.type == type) {
                available.Add(Animator.StringToHash(name));
                return;
            }
        }
        missing.Add($"{name} ({type})");
    }

    void OnEnable() {
        duck.StateChanged += OnStateChanged;
        if (flight != null) flight.Flapped += OnFlapped;
    }

    void OnDisable() {
        duck.StateChanged -= OnStateChanged;
        if (flight != null) flight.Flapped -= OnFlapped;
    }

    void OnStateChanged(DuckState from, DuckState to) => SetInt(StateId, (int)to);

    // Q = asa esquerda, E = asa direita
    void OnFlapped(bool leftWing) => SetTrigger(leftWing ? FlapLeftId : FlapRightId);

    void Update(){
        var v = duck.Body.linearVelocity;
        float speedX = Mathf.Abs(v.x);

        // Modo: 0 = Ground, 1 = Ballistic, 2 = Flying, 3 = Water (mesma ordem do MovementModeType)
        SetInt(ModeId, (int)duck.Mode);
        SetFloat(SpeedXId, speedX);
        SetFloat(VelYId, v.y);
        SetBool(FallenId, duck.State == DuckState.Fallen || duck.State == DuckState.GettingUp);

        // Equilíbrio: 0 = reto, 1 = no limite de cair. É o que faz os pés ficarem mais desesperados
        float balance = duck.Mode == MovementModeType.Ground
            ? Mathf.Clamp01(Mathf.Abs(ground.Angle) / movementSettings.maxLeanAngle)
            : 0f;
        if (available.Contains(BalanceId)) animator.SetFloat(BalanceId, balance, 0.1f, Time.deltaTime);

        // O ciclo de andar acelera e desacelera com o pato, para o pé não patinar
        SetFloat(WalkSpeedId, Mathf.Clamp(speedX / walkReferenceSpeed, walkSpeedRange.x, walkSpeedRange.y));
    }

    void SetInt(int id, int value)     { if (available.Contains(id)) animator.SetInteger(id, value); }
    void SetFloat(int id, float value) { if (available.Contains(id)) animator.SetFloat(id, value); }
    void SetBool(int id, bool value)   { if (available.Contains(id)) animator.SetBool(id, value); }
    void SetTrigger(int id)            { if (available.Contains(id)) animator.SetTrigger(id); }
}