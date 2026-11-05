using UnityEngine;

public class SimpleFollow : MonoBehaviour {
    [SerializeField] Transform target;
    [SerializeField] float smooth = 5f;
    [SerializeField] Vector2 offset = new Vector2(3f, 1.5f);

    void LateUpdate(){
        var goal = new Vector3(target.position.x + offset.x, target.position.y + offset.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, goal, smooth * Time.deltaTime);
    }
}