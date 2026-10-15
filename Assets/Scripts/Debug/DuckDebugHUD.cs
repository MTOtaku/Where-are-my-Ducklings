using UnityEngine;

public class DuckDebugHUD : MonoBehaviour {
    [SerializeField] DuckMovement duck;
    [SerializeField] GroundBalanceController ground;

    void OnGUI(){
        string water = duck.CurrentWater != null ? duck.CurrentWater.name : "-";
        GUI.Label(new Rect(10, 10, 700, 25), $"Modo: {duck.Mode}    Estado: {duck.State}");
        GUI.Label(new Rect(10, 30, 700, 25), $"No chão: {duck.IsGrounded}    Vento: {duck.InWind}    Última água: {water}");
        GUI.Label(new Rect(10, 50, 700, 25), $"Inclinação: {ground.Angle:0.0}°    Velocidade X: {duck.Body.linearVelocity.x:0.00}");
    }
}