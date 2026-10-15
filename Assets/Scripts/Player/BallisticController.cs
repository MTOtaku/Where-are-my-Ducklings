using UnityEngine;

public class BallisticController : MonoBehaviour, IMovementMode {
    [SerializeField] MovementSettings movementSettings;
    [SerializeField] Rigidbody2D rigidBody;
    [SerializeField] DuckMovement duckMovement;

    float airTime;
    float peakFallSpeed;

    public DuckState State => DuckState.Airborne;

    public void Enter(){
        airTime = 0f;
        peakFallSpeed = 0f;
        rigidBody.gravityScale = movementSettings.ballisticGravity;   // a velocidade atual é mantida
    }

    public void Exit() { }

    public void Tick(float dt){
        airTime += dt;
        peakFallSpeed = Mathf.Max(peakFallSpeed, -rigidBody.linearVelocity.y);

        if (airTime > movementSettings.ballisticMinAirTime && duckMovement.IsGrounded)
        {
            duckMovement.NotifyLanded(peakFallSpeed);
            duckMovement.SetMode(MovementModeType.Ground);
        }
    }
}