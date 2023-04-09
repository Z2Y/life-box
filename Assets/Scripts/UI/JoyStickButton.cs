using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class JoyStickButton : Button
    {
        // Event delegate triggered on mouse or touch down.
        [SerializeField]
        ButtonDownEvent _onDown = new ();

        [SerializeField]
        ButtonDownEvent _onUp = new();
 
        protected JoyStickButton() { }
 
        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
 
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
 
            _onDown.Invoke();
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
            _onUp.Invoke();
        }

        public void RemoveAllListener()
        {
            onDown.RemoveAllListeners();
            onUp.RemoveAllListeners();
            onClick.RemoveAllListeners();
        }

        public ButtonDownEvent onDown
        {
            get => _onDown;
            set => _onDown = value;
        }
        
        public ButtonDownEvent onUp
        {
            get => _onUp;
            set => _onUp = value;
        }
 
        [Serializable]
        public class ButtonDownEvent : UnityEvent { }
    } 
}