using UnityEngine;

public class AimArrow : MonoBehaviour {
    [SerializeField] SwimController swim;
    [SerializeField] Transform arrow;
    [SerializeField] float maxLength = 2f;

    void LateUpdate(){
        bool show = swim.IsAiming && swim.AimPower01 > 0f;
        arrow.gameObject.SetActive(show);
        if (!show) return;

        arrow.right = swim.AimVector;
        arrow.localScale = new Vector3(swim.AimPower01 * maxLength, 1f, 1f);
    }
}