using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Botão de uma tecla configurável. Coloque no botão (precisa de Button) e escolha a ação em "Target".
// Mostra a tecla atual no texto do botão; ao clicar, espera a próxima tecla (Esc cancela).
[RequireComponent(typeof(Button))]
public class RebindButton : MonoBehaviour {
    public enum Target { LeanLeft, LeanRight, FlapLeft, FlapRight, Quack, Aim }

    [Tooltip("Qual ação este botão reconfigura.")]
    [SerializeField] Target target;
    [Tooltip("Texto do botão onde aparece a tecla. Se ficar vazio, usa o primeiro TextMeshPro filho.")]
    [SerializeField] TMP_Text label;
    [Tooltip("Texto mostrado enquanto espera o jogador apertar a nova tecla.")]
    [SerializeField] string waitingText = "...";

    Button button;
    InputAction action;
    int bindingIndex;
    bool listening;

    void Awake() {
        button = GetComponent<Button>();
        if (label == null) label = GetComponentInChildren<TMP_Text>(true);
    }

    void OnEnable() {
        InputBindings.EnsureLoaded();

        action = InputBindings.FindAction(target.ToString());
        if (action == null) {
            Debug.LogError($"RebindButton: não achei a ação '{target}' no mapa Player.", this);
            return;
        }
        bindingIndex = InputBindings.GetMainBindingIndex(action);

        button.onClick.AddListener(OnClick);
        InputBindings.BindingsChanged += Refresh;
        InputBindings.RebindingChanged += OnRebindingChanged;

        listening = false;
        button.interactable = !InputBindings.IsRebinding;
        Refresh();
    }

    void OnDisable() {
        button.onClick.RemoveListener(OnClick);
        InputBindings.BindingsChanged -= Refresh;
        InputBindings.RebindingChanged -= OnRebindingChanged;

        // Fechou o painel no meio da captura: cancela
        if (listening) InputBindings.CancelRebind();
        listening = false;
    }

    void OnClick() {
        listening = true;
        if (label != null) label.text = waitingText;
        InputBindings.StartRebind(action, bindingIndex);
    }

    // Enquanto algum botão captura, os outros ficam travados
    void OnRebindingChanged(bool busy) {
        button.interactable = !busy;
        if (!busy) {
            listening = false;
            Refresh();
        }
    }

    void Refresh() {
        if (action == null || label == null) return;
        if (listening && InputBindings.IsRebinding) return;   // mantém o "..." enquanto espera
        label.text = InputBindings.GetDisplay(action, bindingIndex);
    }
}