using UnityEngine;

public class DuckAnimatorBridge : MonoBehaviour {
    [SerializeField] DuckMovement duck;
    [SerializeField] Animator animator;

    static readonly int StateId = Animator.StringToHash("State");
    static readonly int SpeedXId = Animator.StringToHash("SpeedX");
    static readonly int VelYId = Animator.StringToHash("VelY");

    void OnEnable()  => duck.StateChanged += OnStateChanged;
    void OnDisable() => duck.StateChanged -= OnStateChanged;

    void OnStateChanged(DuckState from, DuckState to) => animator.SetInteger(StateId, (int)to);

    void Update(){
        var v = duck.Body.linearVelocity;
        animator.SetFloat(SpeedXId, Mathf.Abs(v.x));
        animator.SetFloat(VelYId, v.y);     
    }
}