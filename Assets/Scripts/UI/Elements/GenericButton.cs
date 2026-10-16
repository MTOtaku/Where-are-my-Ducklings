using System;
using UnityEngine;
using UnityEngine.UI;

namespace RockPaperPistol.UI.Elements
{
    public class GenericButton : MonoBehaviour
    {
        private Action _clickAction;
        private Action<int, GenericButton> _clickSelectedAction;

        [SerializeField] private Button _button;

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
            _clickAction?.Invoke();
        }

        public void ClickSelectedButton(int buttonIndex)
        {
            _clickSelectedAction?.Invoke(buttonIndex, this);
        }
        public void SetInteractableButtonState(bool isInteractable)
        {
            _button.interactable = isInteractable;
        }
    }
}