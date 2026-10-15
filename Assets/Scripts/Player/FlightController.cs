using UnityEngine;

public class FlightController : MonoBehaviour, IMovementMode {
    public enum Wings { Neutral, Open, Closed }

    [SerializeField] MovementSettings movementSettings;
    [SerializeField] Rigidbody2D rigidBody;
    [SerializeField] Transform visual;
    [SerializeField] DuckInput input;
    [SerializeField] DuckMovement duckMovement;
    
    public DuckState State => WingState switch {
        Wings.Open => DuckState.FlyGlide,
        Wings.Closed => DuckState.FlyDive,
        _ => DuckState.FlyNeutral
    };
    
    public Wings WingState { get; private set; }  
    float airTime;
    float facing = 1f;

    public void Enter(){
        airTime = 0f;
        float vx = rigidBody.linearVelocity.x;
        if (Mathf.Abs(vx) > 0.1f) facing = Mathf.Sign(vx);
    }

    public void Exit() { visual.localRotation = Quaternion.identity; }

    public void Tick(float dt){
        airTime += dt;

        // Arrasto vertical em fração da altura da tela (independe de resolução e de dispositivo)
        float frac = Mathf.Clamp(input.ConsumePointerDelta().y / Screen.height,
                                 -movementSettings.maxFlapPerStep, movementSettings.maxFlapPerStep);

        if (frac > movementSettings.wingDeadzone)       WingState = Wings.Open;    // para cima: asas abertas, planando
        else if (frac < -movementSettings.wingDeadzone) WingState = Wings.Closed;  // para baixo: asas fechadas, mergulhando
        else                             WingState = Wings.Neutral;

        rigidBody.gravityScale = WingState switch  {
            Wings.Open => movementSettings.glideGravity,
            Wings.Closed => movementSettings.diveGravity,
            _ => movementSettings.neutralGravity
        };

        var v = rigidBody.linearVelocity;
        v.y += Mathf.Abs(frac) * movementSettings.flapThrustPerScreen;          // bater de asas gera sustentação
        if (duckMovement.InWind) v.y += movementSettings.windLift * dt;
        v.x = Mathf.MoveTowards(v.x, facing * movementSettings.airForwardSpeed, movementSettings.airAccel * dt);
        rigidBody.linearVelocity = Vector2.ClampMagnitude(v, movementSettings.maxAirSpeed);

        visual.localRotation = Quaternion.Euler(0f, 0f, Mathf.Clamp(v.y * 4f, -40f, 40f) * facing);

        // Finaliza o Voo
        if (airTime > movementSettings.minAirTime && duckMovement.IsGrounded) {
            duckMovement.NotifyLanded(-rigidBody.linearVelocity.y);
            duckMovement.SetMode(MovementModeType.Ground);
        }
    }
}