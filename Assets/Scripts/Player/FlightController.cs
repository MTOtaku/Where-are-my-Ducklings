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
    [SerializeField] private float flapImpulseForce = 13f;         // Força do impulso, na direção em que o pato aponta
    [SerializeField] private float glideDuration = 0.45f;          // Quanto tempo o impulso dura sustentando o voo
    [SerializeField] private float rotationPerFlap = 25f;          // Graus que CADA batida gira o pato (para o lado oposto da asa)
    [SerializeField] private float maxSafeAngle = 80f;             // Ângulo limite antes de cair em Ragdoll
    [SerializeField] private float landingMaxAngle = 35f;          // Pousar mais torto que isso derruba o pato
    [SerializeField] private bool ragdollOnRepeatedWing = false;   // Regra extra, NÃO está no texto: mesma asa duas vezes derruba

    private enum LastPressedKey { None, Left, Right }
    private LastPressedKey lastKey = LastPressedKey.None;

    private float glideTimer = 0f;
    private float currentAirRotation = 0f;   // +Z = anti-horário = pato inclinado para a ESQUERDA
    private float airTime = 0f;

    // Flags preenchidas pelos eventos de input e consumidas no passo de física
    private bool frameLeftPressed = false;
    private bool frameRightPressed = false;

    public void Enter(){
        airTime = 0f;
        WingState = Wings.Neutral;
        glideTimer = 0f;
        
        currentAirRotation = visual.localRotation.eulerAngles.z;
        if (currentAirRotation > 180f) currentAirRotation -= 360f;

        lastKey = LastPressedKey.None;
        ResetFrameInputs();   // não herda batida pendente de outro modo

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

    // Chamado pelo DuckMovement: a batida que iniciou o voo conta como a primeira batida
    public void RegisterTakeoffFlap(bool leftWing){
        if (leftWing) frameLeftPressed = true;
        else frameRightPressed = true;
    }

    // Aviso no Console: o impulso é cortado por maxAirSpeed em ApplyFlightPhysics, então valores acima dele não têm efeito
    private void OnValidate(){
        if (movementSettings != null && flapImpulseForce > movementSettings.maxAirSpeed)
            Debug.LogWarning($"FlightController: flapImpulseForce ({flapImpulseForce}) é maior que maxAirSpeed ({movementSettings.maxAirSpeed}). " +
                             "A velocidade real da batida fica limitada a maxAirSpeed.", this);
    }

    public void Tick(float dt){
        airTime += dt;

        // Q e E no mesmo passo de física: nada acontece
        if (frameLeftPressed && frameRightPressed) {
            ResetFrameInputs();
            return;
        }

        if (frameLeftPressed || frameRightPressed) {
            bool leftWing = frameLeftPressed;
            var wing = leftWing ? LastPressedKey.Left : LastPressedKey.Right;
            ResetFrameInputs();

            if (ragdollOnRepeatedWing && lastKey == wing) {
                TriggerRagdoll(leftWing ? "Asa esquerda batida duas vezes seguidas" : "Asa direita batida duas vezes seguidas");
                return;
            }

            lastKey = wing;
            ApplyFlap(leftWing);
        }

        // Ângulo limite: o pato aponta para baixo demais e cai em queda livre
        if (Mathf.Abs(currentAirRotation) > maxSafeAngle) {
            TriggerRagdoll($"Ângulo limite atingido ({Mathf.RoundToInt(currentAirRotation)}°)");
            return;
        }

        // Sustentação aérea
        if (glideTimer > 0f) {
            glideTimer -= dt;
            WingState = Wings.Open;
        }
        else {
            WingState = Wings.Neutral;
            if (!duckMovement.InWind && rigidBody.linearVelocity.y < -9f) {
                TriggerRagdoll("Queda sem sustentação (sem bater asas)");
                return;
            }
        }

        ApplyFlightPhysics(dt);
    }

    private void ApplyFlap(bool leftWing){
        glideTimer = glideDuration;

        // 1) Impulso na direção em que o pato está apontado (na primeira batida o ângulo é 0: reto para cima)
        float rad = currentAirRotation * Mathf.Deg2Rad;
        Vector2 pointing = new Vector2(-Mathf.Sin(rad), Mathf.Cos(rad));
        rigidBody.linearVelocity = pointing * flapImpulseForce;

        // 2) Giro para o lado OPOSTO da asa: asa direita gira para a esquerda (+Z), asa esquerda para a direita (-Z)
        currentAirRotation += (leftWing ? -1f : 1f) * rotationPerFlap;

        // Facing acompanha para onde o pato aponta depois do giro (usado por animação e HUD)
        float lean = -Mathf.Sin(currentAirRotation * Mathf.Deg2Rad);
        if (Mathf.Abs(lean) > 0.05f) Facing = Mathf.Sign(lean);
    }

    private void ApplyFlightPhysics(float dt){
        visual.localRotation = Quaternion.Euler(0f, 0f, currentAirRotation);
        
        rigidBody.gravityScale = WingState switch {
            Wings.Open => movementSettings.glideGravity * 0.7f,
            _ => movementSettings.neutralGravity
        };

        Vector2 v = rigidBody.linearVelocity;

        if (duckMovement.InWind) v.y += movementSettings.windLift * dt;

        if (WingState == Wings.Open) {
            // O planeio segue para onde o pato aponta: inclinado para a esquerda, desliza para a esquerda
            float targetX = -Mathf.Sin(currentAirRotation * Mathf.Deg2Rad) * movementSettings.glideForwardSpeed;
            v.x = Mathf.MoveTowards(v.x, targetX, movementSettings.airAccel * dt);
        }
        else {
            v.x = Mathf.MoveTowards(v.x, 0f, (movementSettings.airAccel * 0.4f) * dt);
        }

        rigidBody.linearVelocity = Vector2.ClampMagnitude(v, movementSettings.maxAirSpeed);
        
        if (airTime > movementSettings.minAirTime && duckMovement.IsGrounded && !duckMovement.InWind && v.y < -0.2f) {
            if (Mathf.Abs(currentAirRotation) > landingMaxAngle) {
                TriggerRagdoll("Pousou torto com o bico no chão");
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