using System;
using UnityEngine;
using UnityEngine.InputSystem;

// Tudo sobre as teclas configuráveis: carregar, salvar, resetar e fazer o rebind.
// É estático de propósito: funciona na tela de opções e dentro da fase.
public static class InputBindings {
    public const string PlayerPrefsKey = "duck_bindings";
    const string MapName = "Player";

    // Alguma tecla mudou (ou foi resetada): os botões atualizam o texto
    public static event Action BindingsChanged;
    // Começou (true) ou terminou (false) uma captura de tecla: os botões travam/destravam
    public static event Action<bool> RebindingChanged;

    public static bool IsRebinding { get; private set; }

    static bool loaded;
    static InputActionRebindingExtensions.RebindingOperation current;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() {
        loaded = false;
        current = null;
        IsRebinding = false;
        BindingsChanged = null;
        RebindingChanged = null;
    }

    // Aplica as teclas salvas assim que o jogo abre, para valer em qualquer cena
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void LoadAtStartup() => EnsureLoaded();

    // Carrega as teclas salvas UMA vez por execução (pode ser chamado à vontade)
    public static void EnsureLoaded() {
        if (loaded) return;

        InputActionAsset asset = InputSystem.actions;
        if (asset == null) return;

        string json = PlayerPrefs.GetString(PlayerPrefsKey, "");
        if (!string.IsNullOrEmpty(json)) asset.LoadBindingOverridesFromJson(json);
        loaded = true;
    }

    public static InputAction FindAction(string actionName) {
        InputActionAsset asset = InputSystem.actions;
        return asset == null ? null : asset.FindAction($"{MapName}/{actionName}", false);
    }

    // Índice do binding "de verdade" da ação (ignora composites)
    public static int GetMainBindingIndex(InputAction action) {
        for (int i = 0; i < action.bindings.Count; i++)
            if (!action.bindings[i].isComposite && !action.bindings[i].isPartOfComposite) return i;
        return 0;
    }

    // Texto da tecla atual, ex.: "Q", "Right Button"
    public static string GetDisplay(InputAction action, int bindingIndex) =>
        action.GetBindingDisplayString(bindingIndex, InputBinding.DisplayStringOptions.DontIncludeInteractions);

    public static void StartRebind(InputAction action, int bindingIndex) {
        CancelRebind();

        // O rebind exige a ação desligada. Depois devolve ao estado em que estava
        bool wasEnabled = action.enabled;
        action.Disable();

        string oldPath = action.bindings[bindingIndex].effectivePath;

        var operation = action.PerformInteractiveRebinding(bindingIndex)
            .WithCancelingThrough("<Keyboard>/escape")          // Esc cancela
            .WithControlsExcluding("<Pointer>/position")        // movimento do mouse não vale
            .WithControlsExcluding("<Pointer>/delta")
            .WithControlsExcluding("<Mouse>/scroll")
            .OnMatchWaitForAnother(0.1f)
            .WithTimeout(10f);                                  // desiste sozinho depois de 10 s

        operation.OnComplete(op => {
            string newPath = action.bindings[bindingIndex].effectivePath;
            SwapIfConflict(action, bindingIndex, oldPath, newPath);

            Finish(op, action, wasEnabled);
            Save();
            BindingsChanged?.Invoke();
            RebindingChanged?.Invoke(false);
        });

        operation.OnCancel(op => {
            Finish(op, action, wasEnabled);
            RebindingChanged?.Invoke(false);
        });

        current = operation;
        IsRebinding = true;
        RebindingChanged?.Invoke(true);
        operation.Start();
    }

    static void Finish(InputActionRebindingExtensions.RebindingOperation op, InputAction action, bool wasEnabled) {
        op.Dispose();
        current = null;
        IsRebinding = false;
        if (wasEnabled) action.Enable();
    }

    public static void CancelRebind() {
        current?.Cancel();   // dispara o OnCancel acima, que limpa tudo
    }

    // Se outra ação já usava a tecla escolhida, ela fica com a tecla antiga desta (troca)
    static void SwapIfConflict(InputAction action, int bindingIndex, string oldPath, string newPath) {
        if (action.actionMap == null || oldPath == newPath) return;

        foreach (InputAction other in action.actionMap.actions) {
            if (other == action) continue;

            for (int i = 0; i < other.bindings.Count; i++) {
                InputBinding b = other.bindings[i];
                if (b.isComposite || b.isPartOfComposite) continue;
                if (b.effectivePath != newPath) continue;

                other.ApplyBindingOverride(i, oldPath);
            }
        }
    }

    public static void Save() {
        InputActionAsset asset = InputSystem.actions;
        if (asset == null) return;
        PlayerPrefs.SetString(PlayerPrefsKey, asset.SaveBindingOverridesAsJson());
        PlayerPrefs.Save();
    }

    public static void ResetAll() {
        CancelRebind();

        InputActionAsset asset = InputSystem.actions;
        if (asset == null) return;
        asset.RemoveAllBindingOverrides();
        PlayerPrefs.DeleteKey(PlayerPrefsKey);
        PlayerPrefs.Save();   
        BindingsChanged?.Invoke();
    }
}