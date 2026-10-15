using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WindCurrent : MonoBehaviour {
    void Reset() => GetComponent<Collider2D>().isTrigger = true;

    void OnTriggerStay2D(Collider2D c){
        if (c.attachedRigidbody != null && c.attachedRigidbody.TryGetComponent(out DuckMovement d))
            d.NotifyWind();
    }
}