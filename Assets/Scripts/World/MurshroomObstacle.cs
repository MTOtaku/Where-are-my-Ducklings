using UnityEngine;

public class MushroomObstacle : MonoBehaviour {
    [Header("Lançamento")]

    [Tooltip("Força aplicada ao jogador na direção para a qual o cogumelo aponta.")]
    [SerializeField] private float launchForce = 15f;

    [Tooltip("Tempo mínimo entre ativações do cogumelo.")]
    [SerializeField] private float activationCooldown = 0.5f;

    [Tooltip("Se ativado, o impulso substitui a velocidade atual do jogador.")]
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

        // Entra no estado Ballistic antes de aplicar o impulso.
        duck.SetMode(MovementModeType.Ballistic, "Lançado pelo cogumelo");

        if (replaceVelocity)
            rb.linearVelocity = Vector2.zero;

        rb.AddForce(launchDirection * launchForce, ForceMode2D.Impulse);
    }
}