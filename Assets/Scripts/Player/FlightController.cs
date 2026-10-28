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
    // Avisa a cada batida de asa APLICADA (true = esquerda/Q, false = direita/E). Usado pela animação
    public event System.Action<bool> Flapped;
    public DuckState State => WingState switch {
        Wings.Open => DuckState.FlyGlide,
        Wings.Closed => DuckState.FlyDive,
        _ => DuckState.Airborne 
    };

    [Header("Configurações Aérea")]
    [Tooltip("Velocidade dada pela batida, na direção em que o pato aponta (m/s).")]
    [SerializeField] private float flapImpulseForce = 13f;
    [Tooltip("Quanto tempo (segundos) cada batida mantém o pato planando.")]
    [SerializeField] private float glideDuration = 0.45f;
    [Tooltip("Graus que CADA batida gira o pato, para o lado oposto da asa.")]
    [SerializeField] private float rotationPerFlap = 25f;
    [Tooltip("Desvio máximo (graus) da direção do vento em que o pato decolou. Passou disso, vira Ragdoll. Com vento para cima, 0° é reto para cima.")]
    [SerializeField] private float maxSafeAngle = 80f;
    [Tooltip("Pousar mais torto que isso (graus em relação a de pé) derruba o pato.")]
    [SerializeField] private float landingMaxAngle = 35f;
    [Tooltip("Regra extra: bater a mesma asa duas vezes seguidas derruba o pato.")]
    [SerializeField] private bool ragdollOnRepeatedWing = false;

    private enum LastPressedKey { None, Left, Right }
    private LastPressedKey lastKey = LastPressedKey.None;

    private float glideTimer = 0f;
    private float currentAirRotation = 0f;   // +Z = anti-horário = pato inclinado para a ESQUERDA
    private float referenceAngle = 0f;       // direção do vento na decolagem: o voo começa apontando para cá
    private float airTime = 0f;
    private float outsideWindTime = 0f;      // tempo seguido voando fora da área de vento

    // Flags preenchidas pelos eventos de input e consumidas no passo de física
    private bool frameLeftPressed = false;
    private bool frameRightPressed = false;

    public void Enter(){
        airTime = 0f;
        outsideWindTime = 0f;
        WingState = Wings.Neutral;
        glideTimer = 0f;

        // O voo começa apontando na direção do vento da área em que o pato decolou
        // (0° = cima, +90° = esquerda, -90° = direita, 180° = baixo)
        referenceAngle = duckMovement.InWind ? duckMovement.WindAngle : 0f;
        currentAirRotation = referenceAngle;

        lastKey = LastPressedKey.None;
        ResetFrameInputs();   // não herda batida pendente de outro modo

        float vx = rigidBody.linearVelocity.x;
        if (Mathf.Abs(vx) > 0.1f) Facing = Mathf.Sign(vx);
        UpdateFacing();

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
        
        // Limite de voo fora do vento: evita voar pelo mapa para sempre. Dentro de uma área de vento o contador zera
        if (duckMovement.InWind) {
            outsideWindTime = 0f;
        }
        else if (movementSettings.maxFlightTimeOutsideWind > 0f) {
            outsideWindTime += dt;
            if (outsideWindTime >= movementSettings.maxFlightTimeOutsideWind) {
                TriggerRagdoll($"Tempo máximo de voo fora do vento ({movementSettings.maxFlightTimeOutsideWind:0.#}s)");
                return;
            }
        }

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

        // Ângulo limite: medido a partir da direção do vento da decolagem (com vento para cima, é o mesmo que antes)
        float deviation = Mathf.Abs(Mathf.DeltaAngle(referenceAngle, currentAirRotation));
        if (deviation > maxSafeAngle) {
            TriggerRagdoll($"Ângulo limite atingido ({Mathf.RoundToInt(deviation)}° de desvio da direção do vento)");
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
        Flapped?.Invoke(leftWing); 
        glideTimer = glideDuration;

        // 1) Impulso na direção em que o pato está apontado (na primeira batida é a direção do vento)
        float rad = currentAirRotation * Mathf.Deg2Rad;
        Vector2 pointing = new Vector2(-Mathf.Sin(rad), Mathf.Cos(rad));
        rigidBody.linearVelocity = pointing * flapImpulseForce;

        // 2) Giro para o lado OPOSTO da asa: asa direita gira para a esquerda (+Z), asa esquerda para a direita (-Z)
        currentAirRotation += (leftWing ? -1f : 1f) * rotationPerFlap;

        UpdateFacing();
    }

    // Facing acompanha para onde o pato aponta (usado por animação e HUD)
    private void UpdateFacing(){
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

        // O vento empurra para a direção marcada na área (cima, baixo, esquerda, direita ou diagonal)
        if (duckMovement.InWind) v += duckMovement.WindDirection * (movementSettings.windLift * dt);

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
            // Pouso: o pato precisa estar de pé (0°), não importa para que lado o vento da decolagem ia
            if (Mathf.Abs(Mathf.DeltaAngle(currentAirRotation, 0f)) > landingMaxAngle) {
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