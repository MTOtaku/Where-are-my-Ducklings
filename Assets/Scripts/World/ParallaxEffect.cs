using UnityEngine;

public class SimpleParallax : MonoBehaviour
{
    [Tooltip("Arraste a sua Main Camera para cá")]
    public Transform cam;

    [Tooltip("1 = Acompanha a câmera perfeitamente. 0 = Objeto normal. Valores próximos de 1 movem mais devagar.")]
    [Range(-1f, 1f)]
    public float parallaxMultiplierX = 0.5f;

    [Range(-1f, 1f)]
    public float parallaxMultiplierY = 0.5f;

    private Vector3 startPosition;
    private Vector3 camStartPosition;

    void Start()
    {
        // Se a câmera não for atribuída no Inspector, ele pega a câmera principal automaticamente
        if (cam == null) cam = Camera.main.transform;

        // Salva as posições iniciais
        startPosition = transform.position;
        camStartPosition = cam.position;
    }

    void LateUpdate()
    {
        // Calcula a distância que a câmera percorreu desde o início
        Vector3 camTravel = cam.position - camStartPosition;

        // Aplica o multiplicador na distância percorrida
        float newX = startPosition.x + (camTravel.x * parallaxMultiplierX);
        float newY = startPosition.y + (camTravel.y * parallaxMultiplierY);

        // Atualiza a posição, mantendo o Z intacto
        transform.position = new Vector3(newX, newY, transform.position.z);
    }
}