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

        if (flight != null) {
            GUI.Label(new Rect(10, 70, 700, 25),
                $"Asas: {flight.WingState}    Direção: {flight.Facing:+0;-0}    Velocidade Y: {duck.Body.linearVelocity.y:0.00}");
            
            float energy = (float)typeof(FlightController).GetField("verticalEnergy", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(flight);
            float lastSign = (float)typeof(FlightController).GetField("lastMouseYSign", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(flight);
            float timeSinceDir = (float)typeof(FlightController).GetField("timeSinceDirectionChange", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(flight);
            
            float decay = (float)typeof(FlightController).GetField("energyDecaySpeed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(flight);
            float threshold = (float)typeof(FlightController).GetField("stateThreshold", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(flight);
            float flapTime = (float)typeof(FlightController).GetField("flapTimeThreshold", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(flight);

            GUI.Label(new Rect(10, 95, 700, 25), 
                $"[INPUT] Energia Vertical: {energy:0.0} | Último Sinal: {lastSign:+0;-0} | Tempo Inversão: {timeSinceDir:0.00}s");
            
            GUI.Label(new Rect(10, 115, 700, 25), 
                $"[CONFIG] Decay: {decay} | Threshold: {threshold} | Janela Flap: {flapTime}s");
        }
        else {
            GUI.Label(new Rect(10, 70, 700, 25), "(campo Flight vazio: arraste o Duck para ele)");
        }
    }
}
