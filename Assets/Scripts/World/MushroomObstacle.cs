using UnityEngine;

public class MushroomObstacle : MonoBehaviour {
    [Header("Lançamento")]

    [Tooltip("Velocidade (m/s) dada ao jogador na direção para a qual o cogumelo aponta. Altura do pulo ≈ velocidade² / (2 × 9,81 × Ballistic Gravity).")]
    [SerializeField] private float launchForce = 15f;

    [Tooltip("Tempo mínimo entre ativações do cogumelo.")]
    [SerializeField] private float activationCooldown = 0.5f;

    [Tooltip("Se ativado, o lançamento substitui a velocidade atual do jogador. Desativado, soma à velocidade atual.")]
    [SerializeField] private bool replaceVelocity = true;

    private float lastActivationTime = -Mathf.Infinity;

    private void OnCollisionEnter2D(Collision2D collision){
        TryLaunch(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other){
        TryLaunch(other.gameObject);
    }

    private void TryLaunch(GameObject target){
        if (Time.time < lastActivationTime + activationCooldown)
            return;

        DuckMovement duck = target.GetComponentInParent<DuckMovement>();

        if (duck == null)
            return;

        Rigidbody2D rb = duck.Body;

        if (rb == null)
            return;

        lastActivationTime = Time.time;

        // A direção é determinada pela rotação do cogumelo.
        Vector2 launchDirection = transform.up;
        
        duck.SetMode(MovementModeType.Ballistic, "Lançado pelo cogumelo");

        // Velocidade direta (não depende da massa do pato, como nos outros modos de movimento)
        Vector2 launch = launchDirection * launchForce;
        rb.linearVelocity = replaceVelocity ? launch : rb.linearVelocity + launch;
    }
}