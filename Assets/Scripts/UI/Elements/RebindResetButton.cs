using UnityEngine;
using UnityEngine.UI;

// Botão "Restaurar padrão" da tela de rebind: volta todas as teclas ao original.
[RequireComponent(typeof(Button))]
public class RebindResetButton : MonoBehaviour {
    Button button;

    void Awake() => button = GetComponent<Button>();

    void OnEnable() {
        button.onClick.AddListener(OnClick);
        InputBindings.RebindingChanged += OnRebindingChanged;
        button.interactable = !InputBindings.IsRebinding;
    }

    void OnDisable() {
        button.onClick.RemoveListener(OnClick);
        InputBindings.RebindingChanged -= OnRebindingChanged;
    }

    void OnClick() => InputBindings.ResetAll();

    void OnRebindingChanged(bool busy) => button.interactable = !busy;
}