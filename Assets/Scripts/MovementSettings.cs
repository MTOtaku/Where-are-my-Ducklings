using UnityEngine;

[CreateAssetMenu(menuName = "Ducklings/Movement Settings")]
public class MovementSettings : ScriptableObject {
    
    /*
     O estado Ballistic é para quando voce cair ou for arremessado com a agua pelo estilingue, Como se fosse o ragdoll, exemplo:
     "jogador andou até uma borda, e então ele cai da borda"
     "assim que estiver caindo, vai virar o ragdoll, onde não terá o controle, somente quando cair no chão ou quando entrar em algo como agua ou vento"
     */
    
    [Header("Estados")]
    public float coyoteTime = 0.08f;           // tolerância ao perder o chão antes de virar Ballistic
    public float getUpDuration = 0.4f;
    public float ballisticGravity = 3f;
    public float ballisticMinAirTime = 0.1f;
    public float waterReentryCooldown = 0.2f;  // evita re-entrar na água no instante do lançamento
    
    [Header("Terrestre")]
    public float tapStep = 12f;            // graus por toque
    public float heatPenalty = 0.1f;       // apertar rápido aumenta o giro
    public float heatDecay = 2f;
    public float instability = 250f;       // quão rápido tomba sozinho
    public float angularDamping = 1.2f;
    public float maxLeanAngle = 80f;       // passou disso, cai
    public float speedPerDegree = 0.15f;   // inclinação vira velocidade (20° = 3 m/s)
    public float walkAccel = 20f;          // quão rápido a velocidade alcança o alvo
    public float friction = 8f;            // só para frear quando caído
    public float maxWalkSpeed = 5f;
    public float fallenDuration = 2f;
    public float groundGravity = 3f;
    
    [Header("Aéreo")]
    public float takeoffDragFraction = 0.01f;  // arrasto mínimo (fração da altura da tela) para alçar voo
    public float neutralGravity = 1.2f;
    public float glideGravity = 0.4f;          // asas abertas
    public float diveGravity = 2.5f;           // asas fechadas
    public float flapThrustPerScreen = 25f;    // sustentação por altura de tela arrastada
    public float maxFlapPerStep = 0.1f;
    public float airForwardSpeed = 3f;
    public float airAccel = 8f;
    public float maxAirSpeed = 10f;
    public float windLift = 20f;
    public float minAirTime = 0.4f;
    public float dragSmoothTime = 0.08f;        // suavização do arrasto (segundos)
    public float wingEnterSpeed = 0.15f;        // velocidade vertical para trocar a postura (alturas de tela/s)
    public float steerEnterSpeed = 0.25f;       // velocidade horizontal para trocar a direção
    public float steerDominance = 0.6f;         // o arrasto horizontal precisa vencer o vertical por essa proporção
    public float flapThrustDownPerScreen = 0f;  // sustentação ao arrastar para baixo (0 = só o arrasto para cima sobe)
    
    [Header("Aéreo - Planeio (asas abertas)")]
    public float glideMaxFallSpeed = 1.5f;      // velocidade máxima de queda ao planar (0 = mantém a altura)
    public float glideBrake = 30f;              // quão rápido freia a queda ao abrir as asas (m/s²)
    public float glideForwardSpeed = 4f;        // velocidade para frente ao planar
    public float glideDecayMultiplier = 0.5f;   // menor que 1 = a postura de planeio dura mais depois do último arrasto
    
    [Header("Aquático")]
    public float minForce = 4f;
    public float maxForce = 14f;
    public float maxDragScreenFraction = 0.25f; // arrasto que dá força máxima
    public float minDragPixels = 12f;
    public float buoyancyPerForce = 1.2f;       // empuxo proporcional ao impulso
    public float underwaterDrag = 1.5f;
    public float exitMultiplier = 1.4f;         // "água gera impulso maior"
    public float surfaceSpring = 8f;
    public float surfaceDrag = 3f;
    public float launchGravity = 3f;
    
}
