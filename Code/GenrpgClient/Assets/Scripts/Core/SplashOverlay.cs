using System;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Core
{
    public class SplashOverlay : MonoBehaviour
    {

        public GText Header;
        public GText Message;
        public GButton ResetButton;

        public GameObject InfoParent;

        private bool _didInit = false;

        public void Show(UnityAction resetGameAction, string message = null, bool showResetButton = false, string header = null)
        {
            try
            {
                InfoParent.SetActive(!string.IsNullOrEmpty(message));
                Message.text = message;
                Header.text = header;
                ResetButton.gameObject.SetActive(showResetButton);

                if (!_didInit)
                {
                    ResetButton.onClick.RemoveAllListeners();
                    ResetButton.onClick.AddListener(resetGameAction);
                }
            }
            catch (Exception e)
            {
                Debug.Log("EXC: " + e.Message);
            }
        }
    }
}


