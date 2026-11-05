using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/*
 Gerenciador de sub-telas do Menu de Opções (Options_Panel).
 Permite alternar entre o conteúdo principal das opções e a tela de rebind de controles,
 garantindo que ao voltar, o jogador retorne para a tela de opções.
*/
public class OptionsPanelController : MonoBehaviour {
    [Header("Sub-Telas / Containers")]
    [Tooltip("Objeto que contém os elementos visuais principais das opções (sliders, botões gerais etc.).")]
    [SerializeField] private GameObject optionsMainContent;

    [Tooltip("Objeto da tela de Rebind / Controles. Começa desligado e abre ao clicar no botão de rebind.")]
    [SerializeField] private GameObject rebindPanel;

    [Header("Botões (GenericButton)")]
    [Tooltip("Botão dentro das opções que abre a tela de rebind.")]
    [SerializeField] private GenericButton rebindGenericButton;

    [Tooltip("Botão 'Voltar' dentro da tela de rebind que retorna para as opções.")]
    [SerializeField] private GenericButton backFromRebindGenericButton;

    [Header("Botões Padrão UI (Opcional - caso use Button nativo em vez de GenericButton)")]
    [SerializeField] private Button rebindUiButton;
    [SerializeField] private Button backFromRebindUiButton;

    [Header("Foco de Navegação (Gamepad / Teclado)")]
    [Tooltip("Primeiro botão selecionado ao abrir a tela de opções. Opcional.")]
    [SerializeField] private GameObject firstSelectedOptions;

    [Tooltip("Primeiro botão selecionado ao abrir a tela de rebind. Opcional.")]
    [SerializeField] private GameObject firstSelectedRebind;

    private void Awake() {
        // Garante que o painel de rebind comece oculto
        if (rebindPanel != null) {
            rebindPanel.SetActive(false);
        }

        if (optionsMainContent != null) {
            optionsMainContent.SetActive(true);
        }
    }

    private void Start() {
        // Inicializa GenericButtons (mesmo padrão do UIMenuManager e PauseController)
        if (rebindGenericButton != null) {
            rebindGenericButton.Initialize(OpenRebindScreen);
        }

        if (backFromRebindGenericButton != null) {
            backFromRebindGenericButton.Initialize(BackToOptions);
        }

        // Suporte opcional a UnityEngine.UI.Button nativo
        if (rebindUiButton != null) {
            rebindUiButton.onClick.AddListener(OpenRebindScreen);
        }

        if (backFromRebindUiButton != null) {
            backFromRebindUiButton.onClick.AddListener(BackToOptions);
        }
    }

    private void OnEnable() {
        // Sempre que a tela de opções for aberta/ativada, reseta para a visão principal de opções
        ResetToOptionsMain();
    }

    /// <summary>
    /// Abre a tela de rebind de teclas e oculta o conteúdo principal das opções.
    /// </summary>
    public void OpenRebindScreen() {
        if (optionsMainContent != null) {
            optionsMainContent.SetActive(false);
        }

        if (rebindPanel != null) {
            rebindPanel.SetActive(true);
        }

        // Foco de seleção para teclado/gamepad
        if (firstSelectedRebind != null && EventSystem.current != null) {
            EventSystem.current.SetSelectedGameObject(firstSelectedRebind);
        }
    }

    /// <summary>
    /// Fecha a tela de rebind e volta para o menu de opções principal.
    /// </summary>
    public void BackToOptions() {
        if (rebindPanel != null) {
            rebindPanel.SetActive(false);
        }

        if (optionsMainContent != null) {
            optionsMainContent.SetActive(true);
        }

        // Retorna o foco para o primeiro selecionado das opções ou para o botão de abrir rebind
        GameObject targetFocus = firstSelectedOptions != null 
            ? firstSelectedOptions 
            : (rebindGenericButton != null ? rebindGenericButton.gameObject : (rebindUiButton != null ? rebindUiButton.gameObject : null));

        if (targetFocus != null && EventSystem.current != null) {
            EventSystem.current.SetSelectedGameObject(targetFocus);
        }
    }

    /// <summary>
    /// Reseta o estado para a tela principal de opções.
    /// </summary>
    public void ResetToOptionsMain() {
        if (rebindPanel != null) {
            rebindPanel.SetActive(false);
        }

        if (optionsMainContent != null) {
            optionsMainContent.SetActive(true);
        }

        if (firstSelectedOptions != null && EventSystem.current != null) {
            EventSystem.current.SetSelectedGameObject(firstSelectedOptions);
        }
    }

    /// <summary>
    /// Retorna verdadeiro se o jogador estiver atualmente dentro da sub-tela de rebind.
    /// Útil se quiser interceptar o ESC/Voltar no controller pai.
    /// </summary>
    public bool IsInRebindScreen => rebindPanel != null && rebindPanel.activeSelf;
}
