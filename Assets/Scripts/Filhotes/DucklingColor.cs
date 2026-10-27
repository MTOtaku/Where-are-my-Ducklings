using UnityEngine;
public class DucklingColor : MonoBehaviour {
    [Header("Gravata 1")]
    [Tooltip("O objeto Gravata1 (precisa ter SpriteRenderer).")]
    [SerializeField] SpriteRenderer tie1;
    [SerializeField] Color color1 = Color.white;

    [Header("Gravata 2")]
    [Tooltip("O objeto Gravata2 (precisa ter SpriteRenderer).")]
    [SerializeField] SpriteRenderer tie2;
    [SerializeField] Color color2 = Color.white;
    
    void Reset() {
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>(true)) {
            if (sr.name == "Gravata1") tie1 = sr;
            else if (sr.name == "Gravata2") tie2 = sr;
        }
        if (tie1 != null) color1 = tie1.color;
        if (tie2 != null) color2 = tie2.color;
    }

    void Awake() => Apply();
    
    void OnValidate() => Apply();

    [ContextMenu("Aplicar cores")]
    public void Apply() {
        if (tie1 != null) tie1.color = color1;
        if (tie2 != null) tie2.color = color2;
    }
}