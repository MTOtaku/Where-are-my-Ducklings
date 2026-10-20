using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WindCurrent : MonoBehaviour {
    [Header("Direção do vento")]
    [Tooltip("O vento empurra o pato para cima. Não pode ser marcado junto com Baixo.")]
    [SerializeField] bool up = true;
    [Tooltip("O vento empurra o pato para baixo. Não pode ser marcado junto com Cima.")]
    [SerializeField] bool down;
    [Tooltip("O vento empurra o pato para a esquerda. Não pode ser marcado junto com Direita.")]
    [SerializeField] bool left;
    [Tooltip("O vento empurra o pato para a direita. Não pode ser marcado junto com Esquerda.")]
    [SerializeField] bool right;

    // Valores anteriores (escondidos): servem para descobrir QUAL check acabou de ser marcado e desmarcar só ele
    [SerializeField, HideInInspector] bool prevUp = true, prevDown, prevLeft, prevRight;

    // Direção final do vento (normalizada). Cima+Direita = diagonal. Nenhum check = (0, 0)
    public Vector2 Direction {
        get {
            var dir = new Vector2((right ? 1f : 0f) - (left ? 1f : 0f),
                                  (up ? 1f : 0f) - (down ? 1f : 0f));
            return dir.normalized;
        }
    }

    // Mesma convenção do FlightController: 0° = cima, +90° = esquerda, -90° = direita, 180° = baixo
    public float AngleDegrees {
        get {
            var d = Direction;
            return d == Vector2.zero ? 0f : Mathf.Atan2(-d.x, d.y) * Mathf.Rad2Deg;
        }
    }

    void Reset() => GetComponent<Collider2D>().isTrigger = true;

    void OnTriggerStay2D(Collider2D c){
        if (c.attachedRigidbody != null && c.attachedRigidbody.TryGetComponent(out DuckMovement d))
            d.NotifyWind(this);
    }

    // Roda no editor sempre que um valor do Inspector muda: impede lados opostos ao mesmo tempo
    void OnValidate(){
        ResolveOpposites(ref up, ref down, prevUp, prevDown, "Cima", "Baixo");
        ResolveOpposites(ref left, ref right, prevLeft, prevRight, "Esquerda", "Direita");

        prevUp = up; prevDown = down; prevLeft = left; prevRight = right;

        if (!up && !down && !left && !right)
            Debug.LogWarning($"WindCurrent '{name}': nenhuma direção marcada, o vento não vai empurrar o pato.", this);
    }

    static void ResolveOpposites(ref bool a, ref bool b, bool prevA, bool prevB, string nameA, string nameB){
        if (!(a && b)) return;

        // Desmarca o que acabou de ser marcado; se não der para saber, mantém o primeiro
        if (prevB && !prevA) a = false;
        else b = false;

        Debug.LogWarning($"WindCurrent: {nameA} e {nameB} são opostos e não podem ficar marcados juntos.");
    }

    // Seta azul na Scene view mostrando para onde o vento vai
    void OnDrawGizmos(){
        var dir = Direction;
        if (dir == Vector2.zero) return;

        var col = GetComponent<Collider2D>();
        Vector3 center = col != null ? col.bounds.center : transform.position;
        float size = col != null ? Mathf.Min(col.bounds.size.x, col.bounds.size.y) * 0.4f : 1f;

        Vector3 d = dir;
        Vector3 side = new Vector3(-dir.y, dir.x, 0f);
        Vector3 tip = center + d * size;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(center, tip);
        Gizmos.DrawLine(tip, tip - d * size * 0.3f + side * size * 0.2f);
        Gizmos.DrawLine(tip, tip - d * size * 0.3f - side * size * 0.2f);
    }
}