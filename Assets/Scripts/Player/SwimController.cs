using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class SwimController : MonoBehaviour, IMovementMode {
    enum Phase { Floating, Submerged }

    [SerializeField] MovementSettings movementSettings;
    [SerializeField] Rigidbody2D rigidBody;
    [SerializeField] DuckInput input;
    [SerializeField] DuckMovement duckMovement;

    Phase phase;
    bool aiming;
    Vector2 anchor, diveDir;
    float storedForce;

    public bool IsAiming => aiming;
    public Vector2 AimVector { get; private set; }
    public float AimPower01 { get; private set; }

    public DuckState State =>
        aiming ? DuckState.Aiming :
        phase == Phase.Submerged ? DuckState.Diving : DuckState.Floating;

    WaterZone Water => duckMovement.CurrentWater;
    [SerializeField] private EventReference _enterWaterSound;
    [SerializeField] private EventReference _exitWaterSound;

    private EventInstance duckSoundInstance;

    public void Enter(){
        if(Utils.IsMusicPlaying(duckSoundInstance))
            duckSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        duckSoundInstance = Utils.FmodSoundPlay(duckSoundInstance, _enterWaterSound);
        phase = Phase.Floating;
        aiming = false;
        rigidBody.gravityScale = 0f;
        rigidBody.linearVelocity *= 0.2f;
        input.AimStarted += OnAimStarted;
        input.AimReleased += OnAimReleased;
    }

    public void Exit(){
        if(Utils.IsMusicPlaying(duckSoundInstance))
            duckSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        duckSoundInstance = Utils.FmodSoundPlay(duckSoundInstance, _exitWaterSound);
        aiming = false;
        input.AimStarted -= OnAimStarted;
        input.AimReleased -= OnAimReleased;
    }

    void OnAimStarted(){
        if (phase != Phase.Floating) return;
        aiming = true;
        anchor = input.PointerPosition;
        AimPower01 = 0f;
    }

    void OnAimReleased(){
        if (!aiming) return;
        aiming = false;
        if (AimPower01 <= 0f) return;

        float force = Mathf.Lerp(movementSettings.minForce, movementSettings.maxForce, AimPower01);
        Vector2 dir = AimVector;
        rigidBody.linearVelocity = dir * force;

        if (dir.y < 0f) {
            phase = Phase.Submerged;     
            storedForce = force;
            diveDir = dir;
        }
        else {
            duckMovement.SetMode(MovementModeType.Ballistic);  
        }
    }

    void UpdateAim(){
        Vector2 drag = anchor - input.PointerPosition;
        if (drag.magnitude < movementSettings.minDragPixels) { AimPower01 = 0f; return; }

        AimVector = drag.normalized;
        AimPower01 = Mathf.Clamp01(drag.magnitude / (Screen.height * movementSettings.maxDragScreenFraction));
    }

    public void Tick(float dt){
        if (Water == null) { duckMovement.SetMode(MovementModeType.Ballistic); return; }

        // Saiu da área da água (pelo fundo ou pelas laterais): a física da água acaba aqui.
        if (!Water.Contains(rigidBody.position)) {
            duckMovement.SetMode(MovementModeType.Ballistic, "Water: saiu da área da água");
            return;
        }

        if (aiming) UpdateAim();

        var v = rigidBody.linearVelocity;

        if (phase == Phase.Floating) {
            v.x *= 1f - Mathf.Min(1f, movementSettings.surfaceDrag * dt);
            v.y = (Water.SurfaceY - rigidBody.position.y) * movementSettings.surfaceSpring;
        }
        else {
            // Empuxo proporcional à força do impulso
            v += Vector2.up * (movementSettings.buoyancyPerForce * storedForce) * dt;
            v *= 1f - Mathf.Min(1f, movementSettings.underwaterDrag * dt);

            if (rigidBody.position.y >= Water.SurfaceY && v.y > 0f) {
                // Sai da água com o ângulo invertido e impulso maior
                Vector2 outDir = new Vector2(diveDir.x, -diveDir.y).normalized;
                rigidBody.linearVelocity = outDir * storedForce * movementSettings.exitMultiplier;
                duckMovement.SetMode(MovementModeType.Ballistic);
                return;
            }
        }

        rigidBody.linearVelocity = v;
    }
}