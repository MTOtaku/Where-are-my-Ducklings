using UnityEngine;

[CreateAssetMenu(menuName = "Ducklings/Movement Settings")]
public class MovementSettings : ScriptableObject {
    
    /*
     O estado Ballistic é para quando voce cair ou for arremessado com a agua pelo estilingue, Como se fosse o ragdoll, exemplo:
     "jogador andou até uma borda, e então ele cai da borda"
     "assim que estiver caindo, vai virar o ragdoll, onde não terá o controle, somente quando cair no chão ou quando entrar em algo como agua ou vento"
     */
    
    [Header("Estados")]
    [Tooltip("Tolerância (segundos) ao perder o chão antes de virar Ballistic. Evita virar ragdoll em pequenos desníveis.")]
    public float coyoteTime = 0.08f;
    [Tooltip("Duração (segundos) da animação de levantar depois de cair no chão.")]
    public float getUpDuration = 0.4f;
    [Tooltip("Gravidade do pato enquanto está em Ballistic (ragdoll). Maior = cai mais rápido.")]
    public float ballisticGravity = 3f;
    [Tooltip("Tempo mínimo (segundos) em Ballistic antes de poder voltar ao chão. Evita pousar no mesmo instante em que foi lançado.")]
    public float ballisticMinAirTime = 0.1f;
    [Tooltip("Tempo (segundos) depois de sair da água em que não é possível entrar nela de novo. Evita re-entrar na água no instante do lançamento.")]
    public float waterReentryCooldown = 0.2f;
    
    [Header("Terrestre")]
    [Tooltip("Quantos graus o pato inclina a cada toque em A/D.")]
    public float tapStep = 12f;
    [Tooltip("Penalidade por apertar rápido demais: quanto maior, mais o pato gira sozinho ao spammar A/D.")]
    public float heatPenalty = 0.1f;
    [Tooltip("Quão rápido o 'calor' (penalidade de apertar rápido) esfria quando o jogador para de spammar.")]
    public float heatDecay = 2f;
    [Tooltip("Quão rápido o pato tomba sozinho para o lado em que já está inclinado. Maior = mais difícil de equilibrar.")]
    public float instability = 250f;
    [Tooltip("Amortecimento do giro: maior = a inclinação se estabiliza mais rápido e balança menos.")]
    public float angularDamping = 1.2f;
    [Tooltip("Inclinação máxima em graus. Passou disso, o pato cai (estado Fallen).")]
    public float maxLeanAngle = 80f;
    [Tooltip("Quanto de velocidade cada grau de inclinação gera. Ex.: 0.15 -> 20° dá 3 m/s.")]
    public float speedPerDegree = 0.15f;
    [Tooltip("Quão rápido a velocidade alcança a velocidade alvo (aceleração). Maior = resposta mais brusca.")]
    public float walkAccel = 20f;
    [Tooltip("Atrito usado apenas para frear o pato quando ele está caído.")]
    public float friction = 8f;
    [Tooltip("Velocidade máxima de caminhada no chão (m/s).")]
    public float maxWalkSpeed = 5f;
    [Tooltip("Quanto tempo (segundos) o pato fica caído antes de poder se levantar.")]
    public float fallenDuration = 2f;
    [Tooltip("Gravidade do pato quando está no chão.")]
    public float groundGravity = 3f;
    
    [Header("Aéreo")]
    [Tooltip("Gravidade no ar quando o pato não está planando.")]
    public float neutralGravity = 1.2f;
    [Tooltip("Gravidade durante o planeio. Menor = cai mais devagar.")]
    public float glideGravity = 0.4f;
    [Tooltip("Velocidade para frente ao planar (m/s). Vale para o lado em que o pato está inclinado.")]
    public float glideForwardSpeed = 4f;
    [Tooltip("Quão rápido a velocidade no ar alcança a velocidade alvo. Maior = resposta mais brusca.")]
    public float airAccel = 8f;
    [Tooltip("Velocidade máxima no ar (m/s). Mantenha MAIOR que o 'Flap Impulse Force' do FlightController, senão a batida é cortada.")]
    public float maxAirSpeed = 10f;
    [Tooltip("Força de sustentação da corrente de vento. Maior = o vento empurra o pato para cima com mais força.")]
    public float windLift = 20f;
    [Tooltip("Tempo mínimo (segundos) no estado de voo antes de poder pousar. Evita pousar logo depois de decolar.")]
    public float minAirTime = 0.4f;
    
    [Header("Aquático")]
    [Tooltip("Força mínima do estilingue (m/s), usada quando o arrasto é curto.")]
    public float minForce = 4f;
    [Tooltip("Força máxima do estilingue (m/s), usada com arrasto completo.")]
    public float maxForce = 14f;
    [Tooltip("Fração da altura da tela que o arrasto precisa ter para dar a força máxima. Menor = chega na força máxima com menos arrasto.")]
    public float maxDragScreenFraction = 0.25f;
    [Tooltip("Arrasto mínimo (em pixels) para o estilingue contar como um lançamento. Evita lançar sem querer com um clique.")]
    public float minDragPixels = 12f;
    [Tooltip("Quanto o empuxo da água cresce com a força do lançamento. Maior = o pato sobe mais rápido ao mirar para cima.")]
    public float buoyancyPerForce = 1.2f;
    [Tooltip("Resistência da água debaixo d'água. Maior = o pato desacelera mais rápido ao nadar.")]
    public float underwaterDrag = 1.5f;
    [Tooltip("Multiplicador do impulso ao sair da água. Maior que 1 = a água dá um impulso extra ao lançar o pato.")]
    public float exitMultiplier = 1.4f;
    [Tooltip("Força que mantém o pato na superfície da água (efeito de mola). Maior = volta mais rápido para a superfície.")]
    public float surfaceSpring = 8f;
    [Tooltip("Resistência do movimento na superfície da água. Maior = o pato balança menos.")]
    public float surfaceDrag = 3f;
    
}