using UnityEngine;

public class FlightController : MonoBehaviour, IMovementMode {
    public enum Wings { Neutral, Open, Closed }

    [SerializeField] private MovementSettings movementSettings;
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private Transform visual;
    [SerializeField] private DuckInput input; 
    [SerializeField] private DuckMovement duckMovement;

    public Wings WingState { get; private set; }
    public float Facing { get; private set; } = 1f;  
    public DuckState State => WingState switch {
        Wings.Open => DuckState.FlyGlide,
        Wings.Closed => DuckState.FlyDive,
        _ => DuckState.Airborne 
    };

    [Header("Configurações Aérea")]
    [SerializeField] private float flapImpulseForce = 13f;      // Força do impulso diagonal gerado
    [SerializeField] private float glideDuration = 0.45f;      // Quanto tempo o "impulso" dura sustentando o voo
    [SerializeField] private float torqueRotationSpeed = 160f;  // Velocidade da rotação aplicada no momento da batida
    [SerializeField] private float maxSafeAngle = 80f;         // Ângulo limite (para baixo/parafuso) antes de cair em Ragdoll

    // Controle de Ritmo Estrito
    private enum LastPressedKey { None, Left, Right }
    private LastPressedKey lastKey = LastPressedKey.None;

    private float glideTimer = 0f;
    private float currentAirRotation = 0f;
    private float airTime = 0f;

    // Flags para evitar dupla execução ou trapaça se pressionados exatamente no mesmo frame
    private bool frameLeftPressed = false;
    private bool frameRightPressed = false;

    public void Enter(){
        airTime = 0f;
        WingState = Wings.Neutral;
        glideTimer = 0f;
        
        currentAirRotation = visual.localRotation.eulerAngles.z;
        if (currentAirRotation > 180f) currentAirRotation -= 360f;

        lastKey = LastPressedKey.None;

        float vx = rigidBody.linearVelocity.x;
        if (Mathf.Abs(vx) > 0.1f) Facing = Mathf.Sign(vx);

        if (input != null) {
            input.FlapLeftPressed += HandleFlapLeftInput;
            input.FlapRightPressed += HandleFlapRightInput;
        }
    }

    public void Exit(){ 
        if (input != null) {
            input.FlapLeftPressed -= HandleFlapLeftInput;
            input.FlapRightPressed -= HandleFlapRightInput;
        }

        visual.localRotation = Quaternion.identity; 
    }
    
    private void HandleFlapLeftInput() => frameLeftPressed = true;
    private void HandleFlapRightInput() => frameRightPressed = true;

    public void Tick(float dt){
    airTime += dt;
    
    var playerMap = input.Asset.FindActionMap("Player", true);
    bool holdLeft = playerMap.FindAction("FlapLeft").IsPressed();
    bool holdRight = playerMap.FindAction("FlapRight").IsPressed();

    if (holdLeft) {
        // Rotaciona para a esquerda (torque inverso da asa esquerda batendo/segurada)
        currentAirRotation += torqueRotationSpeed * dt;
    }
    if (holdRight) {
        // Rotaciona para a direita (torque inverso da asa direita batendo/segurada)
        currentAirRotation -= torqueRotationSpeed * dt;
    }

    // Se ambos forem pressionados no mesmo frame é anulada
    if (frameLeftPressed && frameRightPressed) {
        ResetFrameInputs();
        return; 
    }
    
    // Processamento dos impulsos físicos das  asas
    if (frameLeftPressed) {
        if (lastKey == LastPressedKey.Left) {
            TriggerRagdoll("Asa esquerda batida duas vezes seguidas");
            return;
        }
        lastKey = LastPressedKey.Left;
        ApplyQWOPPhysicsImpulse(Vector2.left);
    }
    else if (frameRightPressed) {
        if (lastKey == LastPressedKey.Right) {
            TriggerRagdoll("Asa direita batida duas vezes seguidas");
            return;
        }
        lastKey = LastPressedKey.Right;
        ApplyQWOPPhysicsImpulse(Vector2.right);
    }
    
    ResetFrameInputs();
    
    // Verificação de perda de sustentação / Estol por inclinação crítica
    if (Mathf.Abs(currentAirRotation) > maxSafeAngle) {
        TriggerRagdoll($"Ângulo Limite atingido ({Mathf.RoundToInt(currentAirRotation)}°)");
        return;
    }
    
    // Sustentação Aerea
    if (glideTimer > 0f) {
        glideTimer -= dt;
        WingState = Wings.Open;
    }
    else {
        WingState = Wings.Neutral;
        if (!duckMovement.InWind && rigidBody.linearVelocity.y < -9f) {
            TriggerRagdoll("QWOP: Perda total de velocidade vertical.");
            return;
        }
    }

    ApplyFlightPhysics(dt);
}
private void ApplyQWOPPhysicsImpulse(Vector2 sideDirection){
    glideTimer = glideDuration;

    // Converte a rotação atual do sprite visual em radianos
    float rad = currentAirRotation * Mathf.Deg2Rad;
    
    Vector2 bodyUpVector = new Vector2(-Mathf.Sin(rad), Mathf.Cos(rad));
    
    Vector2 pushDirection = (bodyUpVector + sideDirection).normalized;

    // Aplica o impulso diagonal real baseado na inclinação visual!
    rigidBody.linearVelocity = pushDirection * flapImpulseForce;

    if (sideDirection.x != 0) Facing = Mathf.Sign(sideDirection.x);
}

private void ApplyFlightPhysics(float dt){
    // Força a aplicação da rotação apenas no objeto visual (filho)
    visual.localRotation = Quaternion.Euler(0f, 0f, currentAirRotation);
    
    rigidBody.gravityScale = WingState switch {
        Wings.Open => movementSettings.glideGravity * 0.7f,
        _ => movementSettings.neutralGravity
    };

    Vector2 v = rigidBody.linearVelocity;

    if (duckMovement.InWind) v.y += movementSettings.windLift * dt;

    if (WingState == Wings.Open) {
        // Converte o ângulo em direção horizontal para somar à velocidade de planeio lateral
        float inclinationHorizontalPush = -Mathf.Sin(currentAirRotation * Mathf.Deg2Rad);
        
        float targetX = (Facing * movementSettings.glideForwardSpeed) + (inclinationHorizontalPush * movementSettings.glideForwardSpeed);
        v.x = Mathf.MoveTowards(v.x, targetX, movementSettings.airAccel * dt);
    }
    else {
        v.x = Mathf.MoveTowards(v.x, 0f, (movementSettings.airAccel * 0.4f) * dt);
    }

    rigidBody.linearVelocity = Vector2.ClampMagnitude(v, movementSettings.maxAirSpeed);
    
    if (airTime > movementSettings.minAirTime && duckMovement.IsGrounded && !duckMovement.InWind && v.y < -0.2f) {
        if (Mathf.Abs(currentAirRotation) > 25f) {
            TriggerRagdoll("QWOP: Pousou torto com o bico no chão!");
        }
        else {
            duckMovement.NotifyLanded(-v.y);
            duckMovement.SetMode(MovementModeType.Ground, "Flying: Pouso bem sucedido");
        }
    }
}
    private void ResetFrameInputs(){
        frameLeftPressed = false;
        frameRightPressed = false;
    }

    private void TriggerRagdoll(string reason){
        duckMovement.SetMode(MovementModeType.Ballistic, reason);
    }
}
