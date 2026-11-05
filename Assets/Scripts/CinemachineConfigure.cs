using Unity.Cinemachine;
using UnityEngine;

public class CinemachineConfigure : MonoBehaviour
{
    [SerializeField] private CinemachineConfiner2D confiner;

    void OnEnable()
    {
        GameEvents.GameSystem.OnStartLevel += StartedLevel;
    }

    void OnDisable()
    {
        GameEvents.GameSystem.OnStartLevel -= StartedLevel;
    }

    private void StartedLevel(BoxCollider2D collider)
    {
        confiner.BoundingShape2D = collider;
    }
}
