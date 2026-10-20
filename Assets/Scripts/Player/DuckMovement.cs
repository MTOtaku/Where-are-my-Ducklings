using System;
using UnityEngine;

public enum MovementModeType { Ground, Ballistic, Flying, Water }   

public class DuckMovement : MonoBehaviour {
    [SerializeField] GroundBalanceController ground;
    [SerializeField] BallisticController ballistic;
    [SerializeField] FlightController flying;
    [SerializeField] SwimController water;
    [SerializeField] DuckInput input;
    [SerializeField] MovementSettings movementSettings;
    [SerializeField] Rigidbody2D rigidBody;

    [Header("Ground check")]
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundCheckRadius = 0.2f;
    [SerializeField] LayerMask groundMask;

    public event Action OnFell;
    public event Action<DuckState, DuckState> StateChanged;
    public event Action<float> Landed;

    public MovementModeType Mode { get; private set; }
    public DuckState State { get; private set; }
    public WaterZone CurrentWater { get; private set; }
    public Rigidbody2D Body => rigidBody;
    public bool IsGrounded => Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);
    public bool InWind => Time.time - lastWindTime < 0.1f;

    // Direção do último vento em que o pato esteve (só vale enquanto InWind)
    public Vector2 WindDirection { get; private set; } = Vector2.up;
    // Mesma direção como ângulo: 0° = cima, +90° = esquerda, -90° = direita, 180° = baixo
    public float WindAngle { get; private set; }
    
    // Só para o HUD de debug: qual objeto está fazendo o pato contar como "no chão" e por que o modo mudou por último
    public string GroundedBy {
        get {
            var hit = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);
            return hit != null ? hit.name : "-";
        }
    }
    public string LastTransition { get; private set; } = "-";

    // Só para o HUD de debug: quantas vezes o FixedUpdate rodou e qual controller está ativo
    public int FixedTicks { get; private set; }
    public string CurrentController => current == null ? "NULL" : current.GetType().Name;

    IMovementMode current;
    Action takeOffLeft, takeOffRight;
    float lastWindTime = -1f;
    float lastWaterExitTime = -10f;

    void Awake() {
        ground.Fell += () => OnFell?.Invoke();
        
        if (input != null) {
            takeOffLeft = () => TryTakeOffByFlap(true);
            takeOffRight = () => TryTakeOffByFlap(false);
            input.FlapLeftPressed += takeOffLeft;
            input.FlapRightPressed += takeOffRight;
        }
    }

    void OnDestroy() {
        if (input != null) {
            input.FlapLeftPressed -= takeOffLeft;
            input.FlapRightPressed -= takeOffRight;
        }
    }
    
    void Start() => SetMode(MovementModeType.Ground);

    // Chamado pela WindCurrent a cada passo de física enquanto o pato está na área.
    // Se duas áreas se sobrepõem, vale a última que avisou.
    public void NotifyWind(WindCurrent wind) {
        lastWindTime = Time.time;
        WindDirection = wind.Direction;
        WindAngle = wind.AngleDegrees;
    }

    public void NotifyLanded(float impactSpeed) => Landed?.Invoke(impactSpeed);

    public void EnterWater(WaterZone zone){
        CurrentWater = zone;
        bool cooledDown = Time.time - lastWaterExitTime > movementSettings.waterReentryCooldown;
        if (Mode != MovementModeType.Water && cooledDown)
            SetMode(MovementModeType.Water);
    }
    
    public void SetMode(MovementModeType type, string reason = ""){
        LastTransition = $"{Mode} -> {type} | {reason} | chão: {GroundedBy} | t={Time.time:0.0}s";

        if (current != null && Mode == MovementModeType.Water && type != MovementModeType.Water)
            lastWaterExitTime = Time.time;

        current?.Exit();
        Mode = type;
        current = type switch {
            MovementModeType.Flying => flying,
            MovementModeType.Water => water,
            MovementModeType.Ballistic => ballistic,
            _ => ground
        };
        
        input.ClearPointerDelta(); 
    
        current.Enter();
        PublishState();
    }
    
    private void TryTakeOffByFlap(bool leftWing) {
        bool canTakeOff = Mode == MovementModeType.Ground || Mode == MovementModeType.Ballistic;

        if (canTakeOff && InWind) {
            SetMode(MovementModeType.Flying, "decolou: batida de asas iniciada no vento");
            flying.RegisterTakeoffFlap(leftWing);
        }
    }

    void FixedUpdate(){
        FixedTicks++;
        
        current?.Tick(Time.fixedDeltaTime);

        // O delta do mouse só é usado na mira da água; nos outros modos não deixamos acumular
        if (Mode != MovementModeType.Flying) input.ClearPointerDelta();
        PublishState();
    }

    void PublishState(){
        if (current == null) return;
        var newState = current.State;
        if (newState == State) return;

        var old = State;
        State = newState;
        StateChanged?.Invoke(old, newState);
    }
}