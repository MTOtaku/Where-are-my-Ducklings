using UnityEngine;

public class FlightController : MonoBehaviour, IMovementMode {
    public enum Wings { Neutral, Open, Closed }

    [SerializeField] MovementSettings movementSettings;
    [SerializeField] Rigidbody2D rigidBody;
    [SerializeField] Transform visual;
    [SerializeField] DuckInput input;
    [SerializeField] DuckMovement duckMovement;
    
    public Wings WingState { get; private set; }
    public float Facing { get; private set; } = 1f;  // +1 direita, -1 esquerda 
    public Vector2 SmoothVel => smoothVel;  // alturas de tela por segundo

    public DuckState State => WingState switch {
        Wings.Open => DuckState.FlyGlide,
        Wings.Closed => DuckState.FlyDive,
        _ => DuckState.FlyNeutral
    };

    float airTime;
    Vector2 smoothVel;

    public void Enter(){
        airTime = 0f;
        smoothVel = Vector2.zero;
        WingState = Wings.Neutral;

        float vx = rigidBody.linearVelocity.x;
        if (Mathf.Abs(vx) > 0.1f) Facing = Mathf.Sign(vx);
    }

    public void Exit() { visual.localRotation = Quaternion.identity; }

    public void Tick(float dt){
        airTime += dt;

        // Arrasto deste passo em fração da altura da tela (independe de resolução e dispositivo)
        Vector2 raw = Vector2.ClampMagnitude(input.ConsumePointerDelta() / Screen.height, movementSettings.maxFlapPerStep);
        
        float k = 1f - Mathf.Exp(-dt / movementSettings.dragSmoothTime);
        smoothVel = Vector2.Lerp(smoothVel, raw / dt, k);

        // Postura das asas: só muda ao passar do limite no sentido oposto
        if (smoothVel.y > movementSettings.wingEnterSpeed)        WingState = Wings.Open;
        else if (smoothVel.y < -movementSettings.wingEnterSpeed)  WingState = Wings.Closed;

        // Direção: arrastar para o lado escolhe para onde voar, e mantém
        if (Mathf.Abs(smoothVel.x) > movementSettings.steerEnterSpeed &&
            Mathf.Abs(smoothVel.x) > Mathf.Abs(smoothVel.y) * movementSettings.steerDominance)
            Facing = Mathf.Sign(smoothVel.x);

        rigidBody.gravityScale = WingState switch {
            Wings.Open => movementSettings.glideGravity,
            Wings.Closed => movementSettings.diveGravity,
            _ => movementSettings.neutralGravity
        };

        var v = rigidBody.linearVelocity;
        v.y += Mathf.Max(0f, raw.y) * movementSettings.flapThrustPerScreen  // bater de asas para cima gera sustentação
             + Mathf.Max(0f, -raw.y) * movementSettings.flapThrustDownPerScreen;
        if (duckMovement.InWind) v.y += movementSettings.windLift * dt;
        v.x = Mathf.MoveTowards(v.x, Facing * movementSettings.airForwardSpeed, movementSettings.airAccel * dt);
        rigidBody.linearVelocity = Vector2.ClampMagnitude(v, movementSettings.maxAirSpeed);

        visual.localRotation = Quaternion.Euler(0f, 0f, Mathf.Clamp(v.y * 4f, -40f, 40f) * Facing);

        // Tocou o chão: volta ao terrestre e só voa de novo numa outra corrente
        if (airTime > movementSettings.minAirTime && duckMovement.IsGrounded) {
            duckMovement.NotifyLanded(-rigidBody.linearVelocity.y);
            duckMovement.SetMode(MovementModeType.Ground);
        }
    }
}