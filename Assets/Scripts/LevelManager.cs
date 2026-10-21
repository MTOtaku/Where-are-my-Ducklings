using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private BoxCollider2D levelLimits;

    void Start()
    {
        GameEvents.GameSystem.OnStartLevel?.Invoke(levelLimits);
    }
}
