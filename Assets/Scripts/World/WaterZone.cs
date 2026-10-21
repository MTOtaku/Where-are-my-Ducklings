using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WaterZone : MonoBehaviour {
    [Tooltip("Margem (m) acima da superfície em que o centro do pato ainda conta como dentro da água. Vale para entrar e para continuar na água.")]
    [SerializeField] float enterHeightMargin = 1f;

    Collider2D col;

    public float SurfaceY => col.bounds.max.y;

    void Awake() => col = GetComponent<Collider2D>();
    void Reset() => GetComponent<Collider2D>().isTrigger = true;
    public bool Contains(Vector2 point){
        Bounds b = col.bounds;
        return point.x >= b.min.x && point.x <= b.max.x
                                  && point.y >= b.min.y && point.y <= b.max.y + enterHeightMargin;
    }

    void OnTriggerStay2D(Collider2D c){
        if (c.attachedRigidbody == null || !c.attachedRigidbody.TryGetComponent(out DuckMovement d)) return;

        var body = d.Body;
        if (Contains(body.position) && body.linearVelocity.y <= 0.01f)
            d.EnterWater(this);
    }
}