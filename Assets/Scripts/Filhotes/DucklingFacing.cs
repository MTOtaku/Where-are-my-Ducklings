using UnityEngine;
public class DucklingFacing : MonoBehaviour {
    [Tooltip("O objeto que vira (o 'Visual' do filhote). Se ficar vazio, usa este próprio objeto.")]
    [SerializeField] Transform visual;
    [Tooltip("A mamãe (o Player com o DuckQuack). Se ficar vazio, procura na cena.")]
    [SerializeField] Transform mother;
    [Tooltip("Ligado: o desenho original do filhote olha para a DIREITA. Desligado: olha para a ESQUERDA.")]
    [SerializeField] bool spriteFacesRight = true;
    [Tooltip("Distância horizontal mínima (m) até a mamãe para o filhote virar. Evita ficar piscando de lado quando ela está em cima dele.")]
    [SerializeField] float turnDeadZone = 0.2f;

    void Awake() {
        if (visual == null) visual = transform;
        FindMother();
    }

    void FindMother() {
        if (mother != null) return;
        DuckQuack quack = FindFirstObjectByType<DuckQuack>();
        if (quack != null) mother = quack.transform;
    }

    void LateUpdate() {
        if (mother == null) {
            FindMother();    // o Player pode aparecer depois do filhote
            if (mother == null) return;
        }

        float dx = mother.position.x - transform.position.x;
        if (Mathf.Abs(dx) < turnDeadZone) return;    // quase alinhado: mantém o lado atual

        // +1 = a mamãe está à direita, -1 = à esquerda
        float side = Mathf.Sign(dx);
        float desired = spriteFacesRight ? side : -side;

        Vector3 scale = visual.localScale;
        if (Mathf.Sign(scale.x) != desired) {
            scale.x = Mathf.Abs(scale.x) * desired;
            visual.localScale = scale;
        }
    }
}