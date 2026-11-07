using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Controla o pause da fase: tecla Esc/Start, abrir e fechar o menu. Coloque UM por cena de jogo.
// IMPORTANTE: não coloque este script no próprio painel de pause (o painel começa desligado e o script junto).
// Ponha no Canvas ou em um objeto vazio "PauseController" que fique sempre ativo.
public class PauseController : MonoBehaviour {
    [Header("Menu")]
    [Tooltip("Painel do menu de pause. Começa desligado e liga ao pausar. Coloque o IgnorePause nele.")]
    [SerializeField] GameObject pausePanel;
    [Tooltip("Botão que fica selecionado ao abrir o menu (navegação por teclado ou controle). Opcional.")]
    [SerializeField] GameObject firstSelected;

    [Header("Botões (GenericButton)")]
    [Tooltip("Botão Continuar.")]
    [SerializeField] GenericButton resumeButton;
    [Tooltip("Botão Reiniciar fase.")]
    [SerializeField] GenericButton restartButton;
    [Tooltip("Botão Sair para o menu.")]
    [SerializeField] GenericButton quitButton;

    [Header("Comportamento")]
    [Tooltip("Pausa sozinho quando a janela do jogo perde o foco (alt-tab). Desligue se atrapalhar os testes no Editor.")]
    [SerializeField] bool pauseWhenFocusLost = true;
    [Tooltip("Nome da cena do menu principal, usado pelo botão 'Sair para o menu'.")]
    [SerializeField] string menuSceneName = "Title_Screen";

    InputAction pauseAction;

    [SerializeField] EventReference pauseSnapshot;
    EventInstance pauseInstance;
    void Awake() {
        // Ação própria (não depende do mapa 'Player', que fica bloqueado durante o pause)
        pauseAction = new InputAction("Pause", InputActionType.Button);
        pauseAction.AddBinding("<Keyboard>/escape");
        pauseAction.AddBinding("<Gamepad>/start");
        pauseInstance = RuntimeManager.CreateInstance(pauseSnapshot);


        if (pausePanel != null) pausePanel.SetActive(false);
    }

    void Start() {
        // Liga cada botão à sua ação (mesmo padrão do UIMenuManager). O Button já chama GenericButton.ClickButton
        if (resumeButton != null) resumeButton.Initialize(Resume);
        if (restartButton != null) restartButton.Initialize(RestartLevel);
        if (quitButton != null) quitButton.Initialize(QuitToMenu);
    }

    void OnEnable() {
        pauseAction.performed += OnPausePressed;
        pauseAction.Enable();
        PauseManager.PauseChanged += OnPauseChanged;
    }

    void OnDisable() {
        pauseAction.performed -= OnPausePressed;
        pauseAction.Disable();
        PauseManager.PauseChanged -= OnPauseChanged;

        // Se este controlador sumir com o jogo pausado, não deixa o jogo travado
        if (PauseManager.IsPaused) PauseManager.Resume();
    }

    void OnDestroy(){
        pauseInstance.release();
        pauseAction?.Dispose();
    }
    void OnApplicationFocus(bool hasFocus) {
        if (!hasFocus && pauseWhenFocusLost && CanPause()) PauseManager.Pause();
    }

    void OnPausePressed(InputAction.CallbackContext _) {
        if (PauseManager.IsPaused) PauseManager.Resume();
        else if (CanPause()) PauseManager.Pause();
    }

    // Não pausa durante a vitória (a contagem do fade já começou)
    bool CanPause() {
        LevelObjective level = LevelObjective.Instance;
        return level == null || (!level.IsCompleted && !level.IsCountingDown);
    }

    void OnPauseChanged(bool paused) {
        if (pausePanel != null) pausePanel.SetActive(paused);

        if (paused)
        {
            pauseInstance.start(); 
        }
        else
        {
            pauseInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }

        if (paused && firstSelected != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(firstSelected);
    }

    // ---- Ações dos botões --------------------------------------------------------------------------

    public void Resume() => PauseManager.Resume();

    public void RestartLevel() {
        GameEvents.UI.OnFadeAnimation?.Invoke(false, RestartLevelAfterFade);
    }

    private void RestartLevelAfterFade()
    {
        PauseManager.Resume();
        Utils.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void QuitToMenu() {
        GameEvents.UI.OnFadeAnimation?.Invoke(false, QuitToMenuAfterFade);
    }

    private void QuitToMenuAfterFade()
    {
        PauseManager.Resume();
        GameEvents.QuitMenu.OnQuit?.Invoke();
        Utils.LoadScene(menuSceneName);
    }
}