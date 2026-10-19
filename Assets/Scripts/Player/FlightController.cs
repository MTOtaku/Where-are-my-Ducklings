using UnityEngine;

public class FlightController : MonoBehaviour, IMovementMode {
    public enum Wings { Neutral, Open, Closed }

    [SerializeField] MovementSettings movementSettings;
    [SerializeField] Rigidbody2D rigidBody;
    [SerializeField] Transform visual;
    [SerializeField] DuckInput input; 
    [SerializeField] DuckMovement duckMovement;
    
    public Wings WingState { get; private set; }
    public float Facing { get; private set; } = 1f;  
    public DuckState State => WingState switch {
        Wings.Open => DuckState.FlyGlide,
        Wings.Closed => DuckState.FlyDive,
        _ => DuckState.Airborne 
    };

    private float verticalEnergy = 0f; 
    private float lastMouseYSign = 0f;
    private float timeSinceDirectionChange = 0f;
    
    // Cronometro pra desativar o voo ao parar de bater asas
    private float timeSinceLastInput = 0f; 

    [Header("Configurações do Sensor de Voo")]
    [SerializeField] float energyDecaySpeed = 50f;     
    [SerializeField] float stateThreshold = 25f;       
    [SerializeField] float flapTimeThreshold = 0.22f;  
    [SerializeField] float maxTimeWithoutInput = 0.5f; // Tempo tolerado sem mexer o mouse antes de cair em queda livre

    float airTime;

    public void Enter(){
        airTime = 0f;
        WingState = Wings.Neutral;
        verticalEnergy = 0f;
        lastMouseYSign = 0f;
        timeSinceDirectionChange = 0f;
        timeSinceLastInput = 0f;

        float vx = rigidBody.linearVelocity.x;
        if (Mathf.Abs(vx) > 0.1f) Facing = Mathf.Sign(vx);
    }

    public void Exit() { visual.localRotation = Quaternion.identity; }

    public void Tick(float dt){
        airTime += dt;
        timeSinceDirectionChange += dt;
        timeSinceLastInput += dt;

        Vector2 rawDelta = input.ConsumePointerDelta();
        float currentY = rawDelta.y;

        // Decaimento natural da energia de voo para caso o jogador pare de bater a asa
        verticalEnergy = Mathf.MoveTowards(verticalEnergy, 0f, energyDecaySpeed * dt);

        if (Mathf.Abs(currentY) > 1f) {
            timeSinceLastInput = 0f; 
            float currentSign = Mathf.Sign(currentY);

            if (currentSign != lastMouseYSign && lastMouseYSign != 0f) {
                if (timeSinceDirectionChange <= flapTimeThreshold) {
                    ApplyFlapForce();
                }
                timeSinceDirectionChange = 0f; 
            }

            lastMouseYSign = currentSign;

            // input para encher o tanque de energia mais rápido ao arrastar
            verticalEnergy += currentY * 0.8f; 
            verticalEnergy = Mathf.Clamp(verticalEnergy, -100f, 100f);
        }

        //Definição dos estados continuos 
        if (verticalEnergy > stateThreshold) {
            WingState = Wings.Open;   // Planar
        }
        else if (verticalEnergy < -stateThreshold) {
            WingState = Wings.Closed; // Mergulhar
        }
        else {
            WingState = Wings.Neutral;

            // Se o jogador não mexer o mouse pelo tempo limite E não estiver no vento: cai em Ballistic
            if (timeSinceLastInput > maxTimeWithoutInput && !duckMovement.InWind) {
                duckMovement.SetMode(MovementModeType.Ballistic);
                return; 
            }
        }
        
        if (Mathf.Abs(rawDelta.x) > movementSettings.steerEnterSpeed) {
            Facing = Mathf.Sign(rawDelta.x);
        }

        ApplyFlightPhysics(dt);
    }
    private void ApplyFlapForce(){
        //Debug.Log("Bateu asas! Força aplicada.");
        var v = rigidBody.linearVelocity;
        v.y += movementSettings.flapThrustPerScreen; 
        rigidBody.linearVelocity = v;
    }

    private void ApplyFlightPhysics(float dt){
        rigidBody.gravityScale = WingState switch {
            Wings.Open => movementSettings.glideGravity,
            Wings.Closed => movementSettings.diveGravity,
            _ => movementSettings.neutralGravity
        };

        var v = rigidBody.linearVelocity;
    
        if (duckMovement.InWind) v.y += movementSettings.windLift * dt;
    
        v.x = Mathf.MoveTowards(v.x, Facing * movementSettings.airForwardSpeed, movementSettings.airAccel * dt);
        rigidBody.linearVelocity = Vector2.ClampMagnitude(v, movementSettings.maxAirSpeed);

        visual.localRotation = Quaternion.Euler(0f, 0f, Mathf.Clamp(v.y * 4f, -40f, 40f) * Facing);
        
        if (airTime > movementSettings.minAirTime && duckMovement.IsGrounded && !duckMovement.InWind && v.y < -0.2f) {
            duckMovement.NotifyLanded(-v.y);
            duckMovement.SetMode(MovementModeType.Ground);
        }
    }

}
