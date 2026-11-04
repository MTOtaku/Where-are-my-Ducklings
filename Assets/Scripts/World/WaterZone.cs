using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WaterZone : MonoBehaviour
{
    [Tooltip("Margem (m) acima da superfície em que o centro do pato ainda conta como dentro da água. Vale para entrar e para continuar na água.")]
    [SerializeField] float enterHeightMargin = 1f;

    [Header("Efeitos Visuais")]
    [Tooltip("Arraste o Prefab do seu Splash de Água (VFX_WaterSplash) aqui")]
    [SerializeField] GameObject waterSplashPrefab;

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

    // Gerencia a troca de controles (Física)
    void OnTriggerStay2D(Collider2D c)
    {
        if (c.attachedRigidbody == null || !c.attachedRigidbody.TryGetComponent(out DuckMovement d)) return;

        var body = d.Body;
        if (Contains(body.position) && body.linearVelocity.y <= 0.01f)
            d.EnterWater(this);
    }

    //Gerencia apenas o impacto visual (Partícula)
    void OnTriggerEnter2D(Collider2D c)
    {
        // Verifica se quem entrou foi o pato
        if (c.attachedRigidbody == null || !c.attachedRigidbody.TryGetComponent(out DuckMovement d)) return;

        // Confirma se o pato está realmente caindo (velocidade Y negativa) e não apenas andando para dentro da água
        if (d.Body.linearVelocity.y < -0.1f)
        {

            // O X é o do pato, mas o Y é travado na linha da superfície da água
            Vector3 splashPosition = new Vector3(d.transform.position.x, SurfaceY, d.transform.position.z);

            if (waterSplashPrefab != null)
            {
                // Instancia a partícula exatamente na linha da água
                GameObject splash = Instantiate(waterSplashPrefab, splashPosition, Quaternion.identity);

                // Destrói o objeto após 1.5 segundos para limpar a memória
                Destroy(splash, 1.5f);
            }
        }
    }
}