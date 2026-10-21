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
        GUI.Label(new Rect(10, 50, 700, 25), $"Inclinação Terrestre: {angle}    Velocidade X: {duck.Body.linearVelocity.x:0.00}");

        if (flight != null) {
            GUI.Label(new Rect(10, 70, 700, 25),
                $"Asas: {flight.WingState}    Direção (Facing): {flight.Facing:+0;-0}    Velocidade Y: {duck.Body.linearVelocity.y:0.00}");
            
            float currentAirRotation = (float)typeof(FlightController)
                .GetField("currentAirRotation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(flight);

            object lastKeyEnum = typeof(FlightController)
                .GetField("lastKey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(flight);
            string ultimaAsa = lastKeyEnum != null ? lastKeyEnum.ToString() : "None";

            float glideTimer = (float)typeof(FlightController)
                .GetField("glideTimer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(flight);

            // Leitura das configurações limitadoras para exibição no HUD
            float maxSafeAngle = (float)typeof(FlightController)
                .GetField("maxSafeAngle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(flight);
            float torqueSpeed = (float)typeof(FlightController)
                .GetField("rotationPerFlap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(flight);
            float flapForce = (float)typeof(FlightController)
                .GetField("flapImpulseForce", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(flight);

            // Renderiza na tela os dados dinâmicos do voo alternado
            GUI.Label(new Rect(10, 95, 700, 25), 
                $"[QWOP DINÂMICO] Rotação do Voo: {currentAirRotation:0.0}° | Última Asa: {ultimaAsa} | Timer Sustentação: {glideTimer:0.00}s");
            
            GUI.Label(new Rect(10, 115, 700, 25), 
                $"[QWOP CONFIGS] Ângulo Limite: {maxSafeAngle}° | Giro por batida: {torqueSpeed} | Força Impulso: {flapForce}");
        }
        else {
            GUI.Label(new Rect(10, 70, 700, 25), "(campo Flight vazio: arraste o Duck para ele)");
        }
        
        GUI.Label(new Rect(10, 140, 900, 25), $"[TRANSIÇÃO] {duck.LastTransition}");
        GUI.Label(new Rect(10, 160, 900, 25), $"[CHÃO AGORA] tocado por: {duck.GroundedBy}");
        var rb = duck.Body;
        var inv = System.Globalization.CultureInfo.InvariantCulture;
        GUI.Label(new Rect(10, 185, 1100, 25), string.Format(inv,
            "[FÍSICA] timeScale={0} | time={1:0.00} | fixedTime={2:0.00} | bodyType={3} | simulated={4} | vel=({5:0.0} ; {6:0.0}) | pos=({7:0.0} ; {8:0.0})",
            Time.timeScale, Time.time, Time.fixedTime, rb.bodyType, rb.simulated,
            rb.linearVelocity.x, rb.linearVelocity.y, rb.position.x, rb.position.y));
        GUI.Label(new Rect(10, 205, 1100, 25),
            $"[DUCKMOVEMENT] enabled={duck.enabled} | FixedUpdate rodou {duck.FixedTicks}x | controller: {duck.CurrentController}");    
    }
}
