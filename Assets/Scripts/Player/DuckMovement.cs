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
    
    // Só para o HUD de debug: qual objeto está fazendo o pato contar como "no chão" e por que o modo mudou por último
    public string GroundedBy {
        get {
            var hit = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);
            return hit != null ? hit.name : "-";
        }
    }
    public string LastTransition { get; private set; } = "-";

    IMovementMode current;
    float lastWindTime = -1f;
    float lastWaterExitTime = -10f;

    void Awake() {
        ground.Fell += () => OnFell?.Invoke();
        
        if (input != null) {
            input.FlapLeftPressed += TryTakeOffByFlap;
            input.FlapRightPressed += TryTakeOffByFlap;
        }
    }
    
    void OnDestroy() {
        if (input != null) {
            input.FlapLeftPressed -= TryTakeOffByFlap;
            input.FlapRightPressed -= TryTakeOffByFlap;
        }
    }
    
    void Start() => SetMode(MovementModeType.Ground);

    public void NotifyWind() => lastWindTime = Time.time;
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
    
    private void TryTakeOffByFlap() {
        bool canTakeOff = Mode == MovementModeType.Ground || Mode == MovementModeType.Ballistic;
        
        if (canTakeOff && InWind) {
            SetMode(MovementModeType.Flying, "decolou: batida de asas iniciada no vento");
        }
    }
    void FixedUpdate(){
        current?.Tick(Time.fixedDeltaTime);
        
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