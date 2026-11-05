using UnityEngine;

// Só visual, vira o para o lado em que ele está indo (esquerda ou direita) invertendo o localScale.x do Visual
public class DuckFacing : MonoBehaviour {
    [SerializeField] DuckMovement duck;
    [Tooltip("O objeto Visual (filho do Player que contém o desenho do pato). NÃO use o Player: o Rigidbody2D e o GroundCheck não devem ser espelhados.")]
    [SerializeField] Transform visual;

    [Tooltip("Marque se o desenho original do pato olha para a DIREITA. Desmarque se ele olha para a esquerda.")]
    [SerializeField] bool spriteFacesRight = true;
    [Tooltip("Velocidade horizontal mínima (m/s) para o pato virar. Evita ficar piscando de lado quando está quase parado ou subindo reto.")]
    [SerializeField] float minSpeedToTurn = 0.3f;

    // +1 = olhando para a direita, -1 = olhando para a esquerda (útil depois para filhotes, etc)
    public float Facing { get; private set; } = 1f;

    // Preenche sozinho os campos ao adicionar o componente
    void Reset() {
        duck = GetComponent<DuckMovement>();
        visual = transform.Find("Visual");
    }

    void Awake() {
        if (duck == null) duck = GetComponent<DuckMovement>();
        if (visual == null) visual = transform.Find("Visual");
    }

    void Update() {
        if (duck == null || visual == null) return;

        // Só troca de lado quando existe velocidade horizontal de verdade; senão mantém o último lado
        float vx = duck.Body.linearVelocity.x;
        if (Mathf.Abs(vx) > minSpeedToTurn) Facing = Mathf.Sign(vx);

        // Se o desenho original olha para a direita, o scale fica positivo quando vai para a direita e negativo para a esquerda.
        // Se olha para a esquerda, é o contrário. Mathf.Abs preserva o tamanho que você configurou no Inspector.
        float side = spriteFacesRight ? Facing : -Facing;
        Vector3 scale = visual.localScale;
        float targetX = Mathf.Abs(scale.x) * side;

        if (!Mathf.Approximately(scale.x, targetX)) {
            scale.x = targetX;
            visual.localScale = scale;
        }
    }
}