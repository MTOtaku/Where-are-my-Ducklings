using UnityEngine;

public class DucklingIndicator : MonoBehaviour {
    [Tooltip("A bolha. Só ganha o 'pulo' de escala ao aparecer.")]
    [SerializeField] RectTransform bubble;
    [Tooltip("A seta que aponta para o filhote quando ele está fora da tela. O sprite deve apontar para a direita.")]
    [SerializeField] RectTransform arrow;
    [SerializeField] CanvasGroup group;

    public RectTransform Rect => (RectTransform)transform;

    void Reset() {
        group = GetComponent<CanvasGroup>();
    }
    
    public void Place(Vector2 canvasPosition, float scale, float alpha, Vector2 direction, float arrowDistance) {
        Rect.anchoredPosition = canvasPosition;

        if (bubble != null) bubble.localScale = Vector3.one * scale;
        if (group != null) group.alpha = alpha;

        if (arrow != null) {
            bool showArrow = direction.sqrMagnitude > 0.0001f;
            if (arrow.gameObject.activeSelf != showArrow) arrow.gameObject.SetActive(showArrow);

            if (showArrow) {
                Vector2 dir = direction.normalized;
                arrow.anchoredPosition = dir * arrowDistance;
                arrow.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
            }
        }
    }

    public void SetVisible(bool visible) => gameObject.SetActive(visible);
}