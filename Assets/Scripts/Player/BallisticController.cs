using UnityEngine;

public class BallisticController : MonoBehaviour, IMovementMode {
    [SerializeField] private MovementSettings movementSettings;
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private DuckMovement duckMovement;
    [SerializeField] private Transform visual; // Adicionado para controlar a rotação de queda

    private float airTime;
    private float peakFallSpeed;
    private float rotationDeadWeightSpeed;

    public DuckState State => DuckState.Airborne;

    public void Enter(){
        airTime = 0f;
        peakFallSpeed = 0f;
        rigidBody.gravityScale = movementSettings.ballisticGravity;

        // Sorteia uma velocidade de rotação cômica de peso morto para o Ragdoll cair girando
        rotationDeadWeightSpeed = Random.Range(-250f, 250f);
    }

    public void Exit() { }

    public void Tick(float dt){
        airTime += dt;
        
        // Armazena com segurança a velocidade máxima vertical de queda antes do impacto
        if (rigidBody.linearVelocity.y < 0f)
        {
            peakFallSpeed = Mathf.Max(peakFallSpeed, Mathf.Abs(rigidBody.linearVelocity.y));
        }

        // Simula o efeito Ragdoll visual: o boneco cai girando descontroladamente feito um peso morto
        visual.Rotate(0f, 0f, rotationDeadWeightSpeed * dt);

        // Checagem de impacto contra o solo de forma robusta
        if (airTime > movementSettings.ballisticMinAirTime && duckMovement.IsGrounded)
        {
            // Aplica o impacto proporcional à velocidade máxima que ele atingiu caindo
            duckMovement.NotifyLanded(peakFallSpeed);
            duckMovement.SetMode(MovementModeType.Ground, "Ballistic: colidiu com o solo");
        }
    }
}