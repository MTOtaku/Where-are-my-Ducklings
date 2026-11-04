using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class DuckInput : MonoBehaviour {
    private const string BindingsKeys = "duck_bindings";

    [SerializeField] private string actionMapName = "Player";
    
    InputActionAsset asset;
    private InputAction leanLeft, leanRight, pointerDelta, pointerPosition, aim, quack, flapLeft, flapRight;
    private Vector2 pointerDeltaAccum;

    // Pause: enquanto bloqueado, nenhum evento de jogo é disparado
    private bool blocked;
    private bool aimHeld;                    // a mira foi iniciada e ainda não foi solta
    private bool releasedWhileBlocked;       // a mira foi solta durante o pause: dispara a soltura ao voltar
    
    public event Action<int> LeanTapped; // -1 pra esquerda, +1 pra direita
    public event Action AimStarted;
    public event Action AimReleased;
    public event Action QuackPressed;
    public event Action FlapLeftPressed;
    public event Action FlapRightPressed;

    public InputActionAsset Asset => asset;
    public Vector2 PointerPosition => pointerPosition.ReadValue<Vector2>();
    public Vector2 PointerDeltaAccum => pointerDeltaAccum;

    public Vector2 ConsumePointerDelta(){
        var direction = pointerDeltaAccum;
        pointerDeltaAccum = Vector2.zero;
        return direction;
    }
    public bool FlapLeftHeld => flapLeft != null && flapLeft.IsPressed();
    public bool FlapRightHeld => flapRight != null && flapRight.IsPressed();
    
    public void ClearPointerDelta() => pointerDeltaAccum = Vector2.zero;

    void Awake(){
        asset = InputSystem.actions;
        if (asset == null) {
            Debug.LogError("Nenhuma Input Action Detectada, Verifique o InputSystem nos project settings");
            enabled = false;
            return;
        }
        
        string json = PlayerPrefs.GetString(BindingsKeys, "");
        if (!string.IsNullOrEmpty(json)) asset.LoadBindingOverridesFromJson(json);
        
        var map = asset.FindActionMap(actionMapName, true);
        leanLeft = map.FindAction("LeanLeft", true);    
        leanRight = map.FindAction("LeanRight", true);
        pointerDelta = map.FindAction("PointerDelta", true);
        pointerPosition = map.FindAction("PointerPosition", true);
        aim = map.FindAction("Aim", true);
        quack = map.FindAction("Quack", true);
        flapLeft = map.FindAction("FlapLeft", true);
        flapRight = map.FindAction("FlapRight", true);
        
        map.Enable();
    }

    void OnEnable(){
        // Pause: bloqueia e libera os controles junto com o PauseManager
        PauseManager.PauseChanged += SetBlocked;
        SetBlocked(PauseManager.IsPaused);

        if (leanLeft == null) return;
        leanLeft.performed += OnLeanLeft;
        leanRight.performed += OnLeanRight;
        aim.performed += OnAimStarted;
        aim.canceled += OnAimReleased;
        quack.performed += OnQuack;
        if (flapLeft != null) flapLeft.performed += OnFlapLeft;
        if (flapRight != null) flapRight.performed += OnFlapRight;
    }
    
    void OnDisable(){
        PauseManager.PauseChanged -= SetBlocked;

        if (leanLeft == null) return;
        leanLeft.performed  -= OnLeanLeft;
        leanRight.performed -= OnLeanRight;
        aim.performed       -= OnAimStarted;
        aim.canceled        -= OnAimReleased;
        quack.performed     -= OnQuack;
        
        if (flapLeft != null) flapLeft.performed -= OnFlapLeft;
        if (flapRight != null) flapRight.performed -= OnFlapRight;
    }
    
    void Update(){
        if (blocked) return;
        pointerDeltaAccum += pointerDelta.ReadValue<Vector2>();
    }

    void SetBlocked(bool value){
        if (blocked == value) return;
        blocked = value;

        // O movimento do mouse durante o pause não pode virar um "tranco" ao voltar
        ClearPointerDelta();

        if (!blocked && releasedWhileBlocked) {
            releasedWhileBlocked = false;
            aimHeld = false;
            AimReleased?.Invoke();
        }
    }

    void OnLeanLeft(InputAction.CallbackContext _){ if (!blocked) LeanTapped?.Invoke(-1); }
    void OnLeanRight(InputAction.CallbackContext _){ if (!blocked) LeanTapped?.Invoke(1); }

    void OnAimStarted(InputAction.CallbackContext _){
        if (blocked) return;
        aimHeld = true;
        AimStarted?.Invoke();
    }

    void OnAimReleased(InputAction.CallbackContext _){
        if (blocked) {
            if (aimHeld) releasedWhileBlocked = true;
            return;
        }
        aimHeld = false;
        AimReleased?.Invoke();
    }

    void OnQuack(InputAction.CallbackContext _){ if (!blocked) QuackPressed?.Invoke(); }
    void OnFlapLeft(InputAction.CallbackContext _){ if (!blocked) FlapLeftPressed?.Invoke(); }
    void OnFlapRight(InputAction.CallbackContext _){ if (!blocked) FlapRightPressed?.Invoke(); }

    // Função de Rebind para quando precisar

    public void StartRebind(InputAction action, int bindingIndex, Action onDone = null){
        action.Disable();

        action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("Mouse")
            .OnComplete(op => {
                op.Dispose();
                action.Enable();
                SaveBindings();
                onDone?.Invoke();
            })
            .OnCancel(op => {
                op.Dispose();
                action.Enable();
            }).Start();
    }
    
    public void SaveBindings() {
        PlayerPrefs.SetString(BindingsKeys, asset.SaveBindingOverridesAsJson());
        PlayerPrefs.Save();
    }

    public void ResetBindings() {
        asset.RemoveAllBindingOverrides();
        PlayerPrefs.DeleteKey(BindingsKeys);
    }
}