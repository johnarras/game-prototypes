using OxDb.Client.Input.Interfaces;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OxDb.Client.UI.SmallUIPieces
{
    public class TabEnterAccelerator : BaseBehaviour, IKeyboardSubsystem
    {

        private IInputService _inputService = null;

        public List<GameObject> SelectableObjects = new List<GameObject>();


        private int _tabIndex = -1;

        public override void Init()
        {
            base.Init();
            _inputService.AddKeyboardSubsystem(this, GetToken());
            IncrementSelectedIndex();
        }

        public void OnKeyPress(Key key)
        {

            if (key == Key.Tab)
            {
                IncrementSelectedIndex();
            }
        }

        public void OnKeyRelease(Key key)
        {
        }

        private void IncrementSelectedIndex()
        {
            if (SelectableObjects.Count < 1)
            {
                return;
            }

            _tabIndex++;
            if (_tabIndex >= SelectableObjects.Count)
            {
                _tabIndex = 0;
            }
            _inputService.SetSelectedObject(SelectableObjects[_tabIndex]);
        }
    }
}
