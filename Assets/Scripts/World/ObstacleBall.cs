using System.Collections;
using UnityEngine;

public class ObstacleBall : MonoBehaviour {
    [Header("Pontos de Respawn")]

    [Tooltip("Ponto A: posição inicial da pedra.")]
    [SerializeField] private Transform pointA;

    [Tooltip("Ponto B: área de chegada que inicia o respawn.")]
    [SerializeField] private Transform pointB;

    [Header("Configurações")]

    [Tooltip("Tempo de espera antes de a pedra reaparecer no ponto A.")]
    [SerializeField] private float respawnDelay = 2f;

    private Rigidbody2D rb;
    private bool isRespawning;

    private void Awake(){
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start(){
        if (pointA == null || pointB == null) {
            Debug.LogError(
                "ObstacleBall: configure os pontos A e B no Inspector.",
                this
            );

            enabled = false;
        }
    }

    public void TriggerRespawn(){
        if (!isRespawning) {
            StartCoroutine(RespawnRoutine());
        }
    }

    private IEnumerator RespawnRoutine(){
        isRespawning = true;
        
        yield return new WaitForSeconds(respawnDelay);

        Respawn();
        
        isRespawning = false;
    }

    private void Respawn(){
        rb.position = pointA.position;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    private void OnDrawGizmosSelected(){
        if (pointA == null || pointB == null)
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(pointA.position, 0.3f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(pointB.position, 0.3f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(pointA.position, pointB.position);
    }
}