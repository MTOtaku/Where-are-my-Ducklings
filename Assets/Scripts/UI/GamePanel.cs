using TMPro;
using UnityEngine;

public class GamePanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ducklingsCountText;

    void OnEnable()
    {
        GameEvents.UI.OnUpdateDucklings += UpdateDucklings;
    }

    void OnDisable()
    {
        GameEvents.UI.OnUpdateDucklings -= UpdateDucklings;
    }

    private void UpdateDucklings(int currentDucklings, int totalDucklings)
    {
        ducklingsCountText.text = $"{currentDucklings}/{totalDucklings}";
    }
}
