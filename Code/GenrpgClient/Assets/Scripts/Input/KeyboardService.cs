using Assets.Scripts.ClientEvents.DataUpdates;
using Assets.Scripts.ClientEvents.UI;
using Assets.Scripts.Input.Interfaces;
using Assets.Scripts.UI.Entities;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Input.Constants;
using Genrpg.Shared.Input.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.Spells.PlayerData.Spells;
using Genrpg.Shared.Spells.Settings.Skills;
using Genrpg.Shared.Spells.Utils;
using Genrpg.Shared.UI.Constants;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Input
{
    internal class InputContainer
    {
        public Key Key;
        public KeyComm Command;
        public int MouseButton = -1;
        public bool IsPressed = false;
    }

    public interface IKeyboardService : IInitializable, IKeyboardSubsystem
    {
        void PerformAction(int actionButtonIndex, Key key);
        bool KeyIsDown(string keyCommand, Key key);
    }

    public class KeyboardService : IKeyboardService
    {
        private IInputService _inputService = null;
        private IPlayerManager _playerManager = null;
        private IDispatcher _dispatcher = null;
        private IScreenService _screenService = null;
        private IClientGameState _gs = null;
        private IGameData _gameData = null;
        private IClientMapObjectManager _objectManager = null;
        private IRealtimeNetworkService _networkService = null;
        private ILogService _logService = null;

        private List<String> _actionKeysTocheck = new List<string>();
        private List<int> _currActionIndexes = new List<int>();
        List<ActiveScreen> screens = null;

        bool screenIsShowing = false;
        private Dictionary<string, InputContainer> _stringInputs = new Dictionary<string, InputContainer>();

        private List<InputContainer> _checkEachFrameInputs = new List<InputContainer>();


        public async Task Initialize(CancellationToken token)
        {
            _dispatcher.AddListener<OnNewGameData>(OnNewGameDataHandler, token);
            await Task.CompletedTask;
        }


        public void OnKeyPress(Key key)
        {
            if (key == Key.None)
            {
                return;
            }

            UpdateUIInputs(key);
            UpdateActionInputs(key);
        }

        private void UpdateUIInputs(Key key)
        {
            screens = null;

            if (key == Key.Escape)
            {
                screens = _screenService.GetAllScreens();
                if (screens != null && screens.Count > 0)
                {
                    _dispatcher.Dispatch(new CloseAllScreens());
                    if (_playerManager.GetPlayerGameObject() != null)
                    {
                        return;
                    }
                }
            }

            if (!_inputService.EditingText())
            {
                InputContainer container = _stringInputs.Values.Where(x => x.Key == key).FirstOrDefault();

                if (container != null)
                {
                    screenIsShowing = false;
                    if (screens == null)
                    {
                        screens = _screenService.GetAllScreens();
                    }

                    foreach (ActiveScreen obj in screens)
                    {
                        ActiveScreen ssi = obj as ActiveScreen;
                        if (ssi == null)
                        {
                            continue;
                        }

                        if (_screenService.GetFullScreenNameFromId(ssi.ScreenId) == container.Command.KeyCommand)
                        {
                            _dispatcher.Dispatch(new CloseScreen(ssi.ScreenId));
                            screenIsShowing = true;
                            break;
                        }
                    }

                    if (!screenIsShowing)
                    {
                        _dispatcher.Dispatch(new OpenScreen(_screenService.GetScreenIdFromName(container.Command.KeyCommand)));
                    }
                }
            }
        }
        private void SetupAbilityIndexes()
        {
            _currActionIndexes = new List<int>();

            _actionKeysTocheck = new List<string>();

            for (int i = InputConstants.MinActionIndex; i <= InputConstants.MaxActionIndex; i++)
            {
                _actionKeysTocheck.Add(KeyComm.ActionPrefix + i);
            }
        }

        private void UpdateActionInputs(Key key)
        {
            _currActionIndexes.Clear();

            if (!_playerManager.Exists())
            {
                return;
            }

            if (_screenService.GetLayerScreen(ScreenLayers.Screens) != null)
            {
                return;
            }

            for (int k = 0; k < _actionKeysTocheck.Count; k++)
            {
                if (KeyPressNow(_actionKeysTocheck[k], key))
                {
                    _currActionIndexes.Add(k + 1);
                }
            }

            if (_currActionIndexes.Count > 0)
            {
                for (int i = 0; i < _currActionIndexes.Count; i++)
                {
                    PerformAction(_currActionIndexes[i], key);
                }
            }
        }

        private DateTime _lastActionTime = DateTime.UtcNow;
        public void PerformAction(int actionIndex, Key key)
        {
            if ((DateTime.UtcNow - _lastActionTime).TotalSeconds < 0.5f)
            {
                return;
            }
            ActionInputData actionInputs = _gs.ch.Get<ActionInputData>();
            ActionInput actionKey = actionInputs.GetInput(actionIndex);
            if (actionKey == null || actionKey.SpellId == 0)
            {
                return;
            }

            Spell spell = _gs.ch.Get<SpellData>().Get(actionKey.SpellId);

            if (spell == null)
            {
                return;
            }

            if (!_playerManager.TryGetUnit(out Unit playerUnit))
            {
                return;
            }

            SkillType skillType = _gameData.Get<SkillTypeSettings>(_gs.ch).Get(spell.Effects.FirstOrDefault()?.SkillTypeId ?? 0);
            if (!_objectManager.GetUnit(playerUnit.TargetId, out Unit target))
            {
                if (skillType.TargetTypeId == TargetTypes.Ally)
                {
                    target = playerUnit;
                }
                else
                {
                    return;
                }
            }

            if (!SpellUtils.IsValidTarget(target, playerUnit.FactionTypeId, skillType.TargetTypeId))
            {
                if (skillType.TargetTypeId == TargetTypes.Ally)
                {
                    target = playerUnit;
                }
                else
                {
                    return;
                }
            }

            if (target == null)
            {
                UpdateTarget(true, key);
                return;
            }
            CastSpell castSpell = new CastSpell()
            {
                SpellId = spell.IdKey,
                TargetId = target.Id,
            };
            _networkService.SendMapMessage(castSpell);
            _lastActionTime = DateTime.UtcNow;
        }


        private void OnNewGameDataHandler(OnNewGameData newGameData)
        {
            SetupAbilityIndexes();
            UpdateFromInputs();
        }

        private void UpdateFromInputs()
        {
            KeyCommData inputList = _gs.ch.Get<KeyCommData>();

            if (inputList == null || inputList.GetData().Count < 1)
            {
                return;
            }

            _stringInputs = new Dictionary<string, InputContainer>();

            foreach (KeyComm item in inputList.GetData())
            {
                if (string.IsNullOrEmpty(item.KeyPress) || string.IsNullOrEmpty(item.KeyCommand))
                {
                    continue;
                }
                item.KeyPress = item.KeyPress.ToLower();
                int mouseButton = -1;
                Key kc = Key.None;
                if (item.KeyPress.Length == 1)
                {
                    try
                    {
                        kc = _inputService.FromChar(item.KeyPress[0]);
                    }
                    catch (Exception e)
                    {
                        _logService.Exception(e, "Bad KeyCode");
                        continue;
                    }
                }
                else if (item.KeyPress == "space")
                {
                    kc = Key.Space;
                }
                else if (item.KeyPress == "esc")
                {
                    kc = Key.Escape;
                }
                else if (item.KeyPress == "tab")
                {
                    kc = Key.Tab;
                }
                else if (item.KeyPress.IndexOf("mouse") == 0)
                {
                    string mouseButtonString = item.KeyPress.Replace("mouse", "");
                    Int32.TryParse(mouseButtonString, out mouseButton);
                }

                if (kc == Key.None && mouseButton < 0)
                {
                    continue;
                }

                if (mouseButton >= 0 && mouseButton < 6)
                {
                    kc = Key.None;
                }

                InputContainer kci = new InputContainer() { Key = kc, Command = item, MouseButton = mouseButton };
                if (!_stringInputs.ContainsKey(item.KeyCommand))
                {
                    _stringInputs[item.KeyCommand] = kci;
                }
            }
        }

        private void UpdateTarget(bool forceTarget, Key key)
        {
            if (KeyPressNow(KeyComm.TargetNext, key) || forceTarget)
            {
                _playerManager.TargetNext();
            }
        }


        public bool KeyPressNow(string keyCommand, Key key)
        {
            if (string.IsNullOrEmpty(keyCommand) || _stringInputs == null || !_stringInputs.ContainsKey(keyCommand))
            {
                return false;
            }
            if (_stringInputs[keyCommand].MouseButton >= 0)
            {
                if (_inputService.MouseClickNow(_stringInputs[keyCommand].MouseButton))
                {
                    return true;
                }
            }

            return _stringInputs[keyCommand].Key == key;
        }

        public bool KeyIsDown(string keyCommand, Key key)
        {

            if (string.IsNullOrEmpty(keyCommand) || _stringInputs == null || !_stringInputs.ContainsKey(keyCommand))
            {
                return false;
            }

            if (_stringInputs[keyCommand].MouseButton >= 0)
            {
                if (_inputService.MouseIsDown(_stringInputs[keyCommand].MouseButton) ||
                    _inputService.MouseClickNow(_stringInputs[keyCommand].MouseButton))
                {
                    return true;
                }
            }

            Key code = _stringInputs[keyCommand].Key;

            return _inputService.IsPressed(code);
        }

        public void OnKeyRelease(Key key)
        {
        }
    }
}


