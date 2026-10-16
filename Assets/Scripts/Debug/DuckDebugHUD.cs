using UnityEngine;

public class DuckDebugHUD : MonoBehaviour {
    [SerializeField] DuckMovement duck;
    [SerializeField] GroundBalanceController ground;
    [SerializeField] FlightController flight;

    void OnGUI(){
        if (duck == null) {
            GUI.Label(new Rect(10, 10, 700, 25), "HUD: o campo Duck está vazio no Inspector");
            return;
        }

        string water = duck.CurrentWater != null ? duck.CurrentWater.name : "-";
        GUI.Label(new Rect(10, 10, 700, 25), $"Modo: {duck.Mode}    Estado: {duck.State}");
        GUI.Label(new Rect(10, 30, 700, 25), $"No chão: {duck.IsGrounded}    Vento: {duck.InWind}    Última água: {water}");

        string angle = ground != null ? $"{ground.Angle:0.0}°" : "(campo Ground vazio)";
        GUI.Label(new Rect(10, 50, 700, 25), $"Inclinação: {angle}    Velocidade X: {duck.Body.linearVelocity.x:0.00}");

        if (flight != null)
            GUI.Label(new Rect(10, 70, 700, 25),
                $"Asas: {flight.WingState}    Direção: {flight.Facing:+0;-0}    Arrasto suave: x {flight.SmoothVel.x:0.00}  y {flight.SmoothVel.y:0.00}");
        else
            GUI.Label(new Rect(10, 70, 700, 25), "(campo Flight vazio: arraste o Duck para ele)");
    }
}