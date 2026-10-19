using System;
using UnityEngine;

public class GroundBalanceController : MonoBehaviour, IMovementMode {
    [SerializeField] MovementSettings movementSettings;
    [SerializeField] Rigidbody2D rigidBody;
    [SerializeField] Transform visual;
    [SerializeField] DuckInput input;
    [SerializeField] DuckMovement duckMovement;

    public event Action Fell;
    public event Action GotUp;

    public DuckState State { get; private set; }
    public float Angle => angle;

    float angle;         // graus, positivo = inclinado para a direita
    float angVel;
    float heat;
    float timer;
    float ungroundedTime;
    float fallSign;
    bool holdBalanced;   // equilibrado por padrão até o primeiro toque

    public void Enter(){
        angle = angVel = heat = ungroundedTime = 0f;
        holdBalanced = true;
        State = DuckState.Idle;
        rigidBody.gravityScale = movementSettings.groundGravity;
        input.LeanTapped += OnLeanTapped;
    }

    public void Exit(){
        input.LeanTapped -= OnLeanTapped;
        visual.localRotation = Quaternion.identity;
    }

    void OnLeanTapped(int dir){
        if (duckMovement.Mode != MovementModeType.Ground) return; 
        if (State == DuckState.Fallen || State == DuckState.GettingUp || !duckMovement.IsGrounded) return;

        holdBalanced = false;
        State = DuckState.Balancing;
        angle += dir * movementSettings.tapStep * (1f + heat * movementSettings.heatPenalty);
        heat += 1f;
    }

    public void Tick(float dt){
        heat = Mathf.Max(0f, heat - movementSettings.heatDecay * dt);
        bool grounded = duckMovement.IsGrounded;
        
        if (duckMovement.Mode == MovementModeType.Ground) {
            ungroundedTime = grounded ? 0f : ungroundedTime + dt;
            if (ungroundedTime > movementSettings.coyoteTime) {
                duckMovement.SetMode(MovementModeType.Ballistic);
                return;
            }
        } else {
            ungroundedTime = 0f; // Mantém resetado se estiver voando, nadando ou já em queda
        }

        bool canWalk = State != DuckState.Fallen && State != DuckState.GettingUp;

        if (grounded && duckMovement.Mode == MovementModeType.Ground) {
            var v = rigidBody.linearVelocity;
            if (canWalk) {
                float target = Mathf.Clamp(angle * movementSettings.speedPerDegree, -movementSettings.maxWalkSpeed, movementSettings.maxWalkSpeed);
                v.x = Mathf.MoveTowards(v.x, target, movementSettings.walkAccel * dt);
            }
            else {
                v.x = Mathf.MoveTowards(v.x, 0f, movementSettings.friction * dt);
            }
            rigidBody.linearVelocity = v;
        }

        switch (State) {
            case DuckState.Fallen:
                timer -= dt;
                if (timer <= 0f) { State = DuckState.GettingUp; timer = movementSettings.getUpDuration; }
                return;

            case DuckState.GettingUp:
                timer -= dt;
                visual.localRotation = Quaternion.Euler(0f, 0f, fallSign * 90f * Mathf.Clamp01(timer / movementSettings.getUpDuration));
                if (timer <= 0f) FinishGetUp();
                return;
        }

        if (holdBalanced) return;
        
        float fallAccel = Mathf.Sin(angle * Mathf.Deg2Rad) * movementSettings.instability;
        angVel += (fallAccel - angVel * movementSettings.angularDamping) * dt;
        angle += angVel * dt;
        visual.localRotation = Quaternion.Euler(0f, 0f, -angle);

        if (Mathf.Abs(angle) > movementSettings.maxLeanAngle) Fall();
    }

    void Fall(){
        State = DuckState.Fallen;
        timer = movementSettings.fallenDuration;
        fallSign = -Mathf.Sign(angle);
        visual.localRotation = Quaternion.Euler(0f, 0f, fallSign * 90f);
        Fell?.Invoke();
    }

    void FinishGetUp(){
        angle = angVel = 0f;
        holdBalanced = true;
        State = DuckState.Idle;
        visual.localRotation = Quaternion.identity;
        GotUp?.Invoke();
    }
}
