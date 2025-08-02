
using Genrpg.Shared.Client.Core;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Core
{
    public class SplashOverlay : MonoBehaviour
    {

        public GText Header;
        public GText Message;
        public GButton ResetButton;

        public GameObject InfoParent;

        private bool _didInit = false;

        private IInitClient _initClient = null;
        public void Show(IInitClient client, string message = null, bool showResetButton = false, string header = null)
        {
            try
            {
                InfoParent.SetActive(!string.IsNullOrEmpty(message));
                _initClient = client;
                Message.text = message;
                Header.text = header;
                ResetButton.gameObject.SetActive(showResetButton);

                if (!_didInit)
                {
                    ResetButton.onClick.AddListener(ResetGame);
                }
            }
            catch (Exception e)
            {
                Debug.Log("EXC: " + e.Message);
            }
        }

        private void ResetGame()
        {
            _initClient.FullResetGame();
        }
    }
}
