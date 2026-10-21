using UnityEngine;

public class ObstacleBallTrigger : MonoBehaviour {
    [Tooltip("Referência à bola que será respawnada.")]
    [SerializeField] private ObstacleBall obstacleBall;

    private void OnTriggerEnter2D(Collider2D other){
        if (obstacleBall != null && other.GetComponent<ObstacleBall>() == obstacleBall) {
            obstacleBall.TriggerRespawn();
        }
    }
}