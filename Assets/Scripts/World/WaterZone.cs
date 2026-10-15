using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WaterZone : MonoBehaviour {
    [SerializeField] float enterHeightMargin = 1f;   // aceita entrar com o centro um pouco acima da superfície

    Collider2D col;

    public float SurfaceY => col.bounds.max.y;

    void Awake() => col = GetComponent<Collider2D>();
    void Reset() => GetComponent<Collider2D>().isTrigger = true;

    void OnTriggerStay2D(Collider2D c){
        if (c.attachedRigidbody == null || !c.attachedRigidbody.TryGetComponent(out DuckMovement d)) return;

        var body = d.Body;
        if (body.position.y <= SurfaceY + enterHeightMargin && body.linearVelocity.y <= 0.01f)
            d.EnterWater(this);
    }
}