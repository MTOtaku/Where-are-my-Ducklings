using System;
using UnityEngine;
using UnityEngine.UI;
using FMOD.Studio;
using FMODUnity;

public class GenericButton : TextLocalize
{
    private Action _clickAction;
    private Action<int, GenericButton> _clickSelectedAction;

    [SerializeField] private Button _button;

    [SerializeField] private EventReference _hoverSound;
    [SerializeField] private EventReference _clickSound;
    private EventInstance buttonSoundInstance;
    public void Initialize(Action buttonAction)
    {
        _clickAction = buttonAction;
    }

    public void Initialize(Action<int, GenericButton> buttonAction)
    {
        _clickSelectedAction = buttonAction;
    }
    
    public void ClickButton()
    {
        PlayButtonMusic(_clickSound);
        _clickAction?.Invoke();
    }

    public void ClickSelectedButton(int buttonIndex)
    {
        PlayButtonMusic(_clickSound);
        _clickSelectedAction?.Invoke(buttonIndex, this);
    }

    public void HoverSelectedButton()
    {
        PlayButtonMusic(_hoverSound);
    }

    private void PlayButtonMusic(EventReference soundReference)
    {
        if (Utils.IsMusicPlaying(buttonSoundInstance))
        {
            buttonSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
        buttonSoundInstance = Utils.FmodSoundPlay(buttonSoundInstance, soundReference);
    }

    public void SetInteractableButtonState(bool isInteractable)
    {
        _button.interactable = isInteractable;
    }
}