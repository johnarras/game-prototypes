using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI.Pointers
{
    public class PointerHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {

        private Action<GameObject> _enterHandler;
        public void OnPointerEnter(PointerEventData eventData)
        {
            _enterHandler?.Invoke(eventData.pointerEnter);
        }

        private Action<GameObject> _exitHandler;
        public void OnPointerExit(PointerEventData eventData)
        {
            _exitHandler?.Invoke(eventData.pointerEnter);
        }

        public void SetEnterExitHandlers(Action<GameObject> enterHandler, Action<GameObject> exitHandler)
        {
            _enterHandler = enterHandler;
            _exitHandler = exitHandler;
        }
    }
}
