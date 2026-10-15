using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class DuckInput : MonoBehaviour {
    private const string BindingsKeys = "duck_bindings";

    [SerializeField] private string actionMapName = "Player";
    
    InputActionAsset asset;
    private InputAction leanLeft, leanRight, pointerDelta, pointerPosition, aim, quack;
    private Vector2 pointerDeltaAccum;
    
    public event Action<int> LeanTapped; // -1 pra esquerda, +1 pra direita
    public event Action AimStarted;
    public event Action AimReleased;
    public event Action QuackPressed;

    public InputActionAsset Asset => asset;
    public Vector2 PointerPosition => pointerPosition.ReadValue<Vector2>();
    public Vector2 PointerDeltaAccum => pointerDeltaAccum;

    public Vector2 ConsumePointerDelta(){
        var direction = pointerDeltaAccum;
        pointerDeltaAccum = Vector2.zero;
        return direction;
    }
    
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
        
        map.Enable();
    }

    void OnEnable(){
        if (leanLeft == null) return;
        leanLeft.performed += OnLeanLeft;
        leanRight.performed += OnLeanRight;
        aim.performed += OnAimStarted;
        aim.canceled += OnAimReleased;
        quack.performed += OnQuack;
    }
    
    void OnDisable(){
        if (leanLeft == null) return;
        leanLeft.performed  -= OnLeanLeft;
        leanRight.performed -= OnLeanRight;
        aim.performed       -= OnAimStarted;
        aim.canceled        -= OnAimReleased;
        quack.performed     -= OnQuack;
    }
    
    void Update() => pointerDeltaAccum += pointerDelta.ReadValue<Vector2>();

    void OnLeanLeft(InputAction.CallbackContext _) => LeanTapped?.Invoke(-1);
    void OnLeanRight(InputAction.CallbackContext _) => LeanTapped?.Invoke(1);
    void OnAimStarted(InputAction.CallbackContext _) => AimStarted?.Invoke();
    void OnAimReleased(InputAction.CallbackContext _) => AimReleased?.Invoke();
    void OnQuack(InputAction.CallbackContext _) => QuackPressed?.Invoke();
    
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
