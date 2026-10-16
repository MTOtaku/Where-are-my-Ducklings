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

    IMovementMode current;
    float lastWindTime = -1f;
    float lastWaterExitTime = -10f;

    void Awake() => ground.Fell += () => OnFell?.Invoke();
    void Start() => SetMode(MovementModeType.Ground);

    public void NotifyWind() => lastWindTime = Time.time;
    public void NotifyLanded(float impactSpeed) => Landed?.Invoke(impactSpeed);

    public void EnterWater(WaterZone zone){
        CurrentWater = zone;
        bool cooledDown = Time.time - lastWaterExitTime > movementSettings.waterReentryCooldown;
        if (Mode != MovementModeType.Water && cooledDown)
            SetMode(MovementModeType.Water);
    }
    
    public void SetMode(MovementModeType type){
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
    
    void FixedUpdate(){
        // Alçar voo: permitido se estiver no chão ou em queda livre (Ballistic)
        bool canTakeOff = Mode == MovementModeType.Ground || Mode == MovementModeType.Ballistic;
    
        // Mede estritamente o arrasto VERTICAL absoluto do mouse
        float verticalDrag = Mathf.Abs(input.PointerDeltaAccum.y) / Screen.height;

        // Se estiver sob efeito do vento E arrastar o mouse na vertical, decola
        if (canTakeOff && InWind && verticalDrag > movementSettings.takeoffDragFraction)
        {
            SetMode(MovementModeType.Flying);
        }

        current?.Tick(Time.fixedDeltaTime);

        // Garante que o input só acumule se estiver voando, limpando nos outros modos
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