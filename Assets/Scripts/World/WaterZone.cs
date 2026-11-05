using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WaterZone : MonoBehaviour
{
    [Tooltip("Margem (m) acima da superfície em que o centro do pato ainda conta como dentro da água. Vale para entrar e para continuar na água.")]
    [SerializeField] float enterHeightMargin = 1f;

    [Header("Efeitos Visuais")]
    [Tooltip("Arraste o Prefab do Splash de Entrada (água subindo) aqui")]
    [SerializeField] GameObject waterSplashPrefab;

    [Tooltip("Arraste o Prefab do Splash de Saída (água respingando ao pular) aqui")]
    [SerializeField] GameObject waterSplashOutPrefab;

    Collider2D col;

    public float SurfaceY => col.bounds.max.y;

    void Awake() => col = GetComponent<Collider2D>();
    void Reset() => GetComponent<Collider2D>().isTrigger = true;

    public bool Contains(Vector2 point)
    {
        Bounds b = col.bounds;
        return point.x >= b.min.x && point.x <= b.max.x
                                  && point.y >= b.min.y && point.y <= b.max.y + enterHeightMargin;
    }

    // Gerencia a troca de controles contínua
    void OnTriggerStay2D(Collider2D c)
    {
        if (c.attachedRigidbody == null || !c.attachedRigidbody.TryGetComponent(out DuckMovement d)) return;

        var body = d.Body;
        if (Contains(body.position) && body.linearVelocity.y <= 0.01f)
            d.EnterWater(this);
    }

    // Gerencia a partícula de ENTRADA (Caindo)
    void OnTriggerEnter2D(Collider2D c)
    {
        if (c.attachedRigidbody == null || !c.attachedRigidbody.TryGetComponent(out DuckMovement d)) return;

        // Confirma se está caindo
        if (d.Body.linearVelocity.y < -0.1f)
        {
            Vector3 splashPosition = new Vector3(d.transform.position.x, SurfaceY, d.transform.position.z);

            if (waterSplashPrefab != null)
            {
                GameObject splash = Instantiate(waterSplashPrefab, splashPosition, Quaternion.identity);
                Destroy(splash, 1.5f);
            }
        }
    }

    //  Gerencia a partícula de SAÍDA (Pulando para fora)
    void OnTriggerExit2D(Collider2D c)
    {
        if (c.attachedRigidbody == null || !c.attachedRigidbody.TryGetComponent(out DuckMovement d)) return;

        // Confirma se o pato está subindo (velocidade Y positiva) ao sair do colisor
        if (d.Body.linearVelocity.y > 0.1f)
        {

            // Nasce na exata superfície do lago, ignorando a altura que o pato já alcançou no pulo
            Vector3 splashPosition = new Vector3(d.transform.position.x, SurfaceY, d.transform.position.z);

            if (waterSplashOutPrefab != null)
            {
                GameObject splashOut = Instantiate(waterSplashOutPrefab, splashPosition, Quaternion.identity);
                Destroy(splashOut, 1.5f);
            }
        }
    }
}