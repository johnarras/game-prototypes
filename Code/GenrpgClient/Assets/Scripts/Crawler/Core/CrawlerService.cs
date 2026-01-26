
using Assets.Scripts.Assets;
using Assets.Scripts.Awaitables;
using Assets.Scripts.ClientEvents;
using Assets.Scripts.ClientEvents.UI;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.UI.Entities;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Combat.Services;
using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Crawler.Currencies.Constants;
using Genrpg.Shared.Crawler.Currencies.Settings;
using Genrpg.Shared.Crawler.Items.Entities;
using Genrpg.Shared.Crawler.Loot.Services;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.Options.Constants;
using Genrpg.Shared.Crawler.Options.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Party.Services;
using Genrpg.Shared.Crawler.Settings;
using Genrpg.Shared.Crawler.Spells.Services;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Crawler.States.StateHelpers;
using Genrpg.Shared.Crawler.Stats.Services;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.LoadSave.Constants;
using Genrpg.Shared.LoadSave.Services;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Serialization.Interfaces;
using Genrpg.Shared.UI.Constants;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Crawler.Services
{
    public class CrawlerService : ICrawlerService
    {
        private IClientUpdateService _updateService = null;
        private ICrawlerStatService _crawlerStatService = null;
        private IScreenService _screenService = null;
        private ICrawlerMoveService _moveService = null;
        protected ILogService _logService = null;
        protected IRepositoryService _repoService = null;
        protected IDispatcher _dispatcher = null;
        protected IClientRandom _rand = null;
        protected ICrawlerCombatService _combatService = null;
        protected ICrawlerWorldService _worldService = null;
        protected ILootGenService _lootGenService = null;
        private IInputService _inputService = null;
        protected IAwaitableService _awaitableService = null;
        private CancellationToken _token;
        private ICrawlerSpellService _spellService = null;
        private ILoadSaveService _loadSaveService = null;
        private ILocalLoadService _localLoadService = null;
        private ITextSerializer _textSerializer = null;
        private ICrawlerOptionsService _optionsService = null;
        private IGameData _gameData = null;
        private IClientGameState _gs = null;
        protected IPartyService _partyService = null;

        public const string SaveFileSuffix = ".sav";
        public const string StartSaveFileName = "Start" + SaveFileSuffix;

        private SetupDictionaryContainer<ECrawlerStates, IStateHelper> _stateHelpers = new SetupDictionaryContainer<ECrawlerStates, IStateHelper>();

        private PartyData _party { get; set; }

        public PartyData GetParty()
        {
            return _party;
        }

        private Stack<CrawlerStateData> _stateStack { get; set; } = new Stack<CrawlerStateData>();

        public async Task Initialize(CancellationToken token)
        {
            _token = token;
            _updateService.AddTokenUpdate(this, UpdateGame, UpdateTypes.Regular, token);
            _updateService.AddTokenUpdate(this, OnLateUpdate, UpdateTypes.Late, token);
            await Task.CompletedTask;
        }


        public CancellationToken GetToken()
        {
            return _token;
        }



        public long GetCrawlerScreenId()
        {
            return ScreenNames.Crawler;
        }

        public void ChangeState(ECrawlerStates crawlerState, CancellationToken token, object extraData = null, ECrawlerStates returnState = ECrawlerStates.None)
        {
            CrawlerStateData stateData = new CrawlerStateData(returnState) { ExtraData = extraData };
            CrawlerStateAction action = new CrawlerStateAction(null, Key.None, crawlerState, extraData: extraData);
            ChangeState(stateData, action, token);
        }

        class FullCrawlerState
        {
            public CrawlerStateData StateData;
            public CrawlerStateAction Action;
        }

        private ConcurrentQueue<FullCrawlerState> _stateQueue = new ConcurrentQueue<FullCrawlerState>();

        public void ChangeState(CrawlerStateData data, CrawlerStateAction action, CancellationToken token)
        {
            _stateQueue.Enqueue(new FullCrawlerState() { Action = action, StateData = data });
        }

        private void OnLateUpdate(CancellationToken token)
        {
            if (_moveService.UpdatingMovement() || _changingState)
            {
                return;
            }

            if (_stateQueue.TryDequeue(out FullCrawlerState fullCrawlerState))
            {
                fullCrawlerState.Action.OnClickAction?.Invoke();

                // This lets you enter commands without changing state.
                if (fullCrawlerState.Action.NextState == ECrawlerStates.DoNotChangeState)
                {
                    return;
                }

                _dispatcher.Dispatch(new HideInfoPanelEvent());

                _awaitableService.ForgetAwaitable(ChangeStateAsync(fullCrawlerState, token));
            }

            if (_inputService.WasPressedThisFrame(Key.Escape))
            {
                ActiveScreen activeScreen = _screenService.GetLayerScreen(ScreenLayers.Screens);
                if (activeScreen != null)
                {
                    _dispatcher.Dispatch(new CloseScreen(activeScreen.ScreenId));
                }
            }
        }

        public CrawlerStateData PopState()
        {
            if (_stateStack.Count > 1)
            {
                _stateStack.Pop();
            }
            CrawlerStateData stateData = _stateStack.Peek();
            _dispatcher.Dispatch(stateData);
            return stateData;
        }

        public CrawlerStateData GetTopLevelState()
        {
            while (_stateStack.Count > 1)
            {
                _stateStack.Pop();
            }
            if (_stateStack.Count < 1)
            {
                return null;
            }
            return _stateStack.Peek();
        }

        public ECrawlerStates GetState()
        {
            if (_stateStack.Count < 1)
            {
                return ECrawlerStates.None;
            }
            return _stateStack.Peek().Id;
        }

        protected bool ShouldDispatchClickKeys()
        {
            IStateHelper helper = GetStateHelper(GetState());
            return helper != null && helper.ShouldDispatchClickKeys();
        }

        private bool _changingState = false;
        private async Awaitable ChangeStateAsync(FullCrawlerState fullState, CancellationToken token)
        {
            _changingState = true;
            await Awaitable.MainThreadAsync();
            try
            {
                CrawlerStateData currData = fullState.StateData;
                CrawlerStateAction action = fullState.Action;
                CrawlerStateData nextStateData = null;
                foreach (CrawlerStateData stackData in _stateStack)
                {
                    if (stackData.Id == action.NextState)
                    {
                        nextStateData = stackData;
                        break;
                    }
                }

                if (nextStateData != null)
                {
                    while (_stateStack.Count > 1 && _stateStack.Peek().Id != nextStateData.Id)
                    {
                        _stateStack.Pop();
                    }
                }

                IStateHelper stateHelper = GetStateHelper(action.NextState);
                if (stateHelper != null)
                {
                    //_logService.Info("ChangeState: " + stateHelper.Key.ToString());
                    nextStateData = await stateHelper.Init(currData, action, token);

                    if (nextStateData.DoNotTransitionToThisState)
                    {
                        _changingState = false;
                        return;
                    }

                    if (stateHelper.IsTopLevelState())
                    {
                        _stateStack.Clear();
                    }
                }

                if (nextStateData != null)
                {
                    nextStateData.HideBigPanels = stateHelper.HideBigPanels();
                    if (nextStateData.ForceNextState)
                    {
                        ChangeState(nextStateData.Id, token, nextStateData.ExtraData);
                    }
                    else
                    {
                        _stateStack.Push(nextStateData);
                        _dispatcher.Dispatch(nextStateData);
                    }
                }
                else
                {
                    _logService.Error("State not found: " + action.NextState);
                }
            }
            catch (Exception e)
            {
                _logService.Exception(e, "CrawlerChangeState");
            }
            _changingState = false;
        }

        private IStateHelper GetStateHelper(ECrawlerStates state)
        {
            if (_stateHelpers.TryGetValue(state, out IStateHelper stateHelper))
            {
                return stateHelper;
            }
            return null;
        }

        private List<CrawlerSaveItem> ConvertItemsFromGameToSave(PartyData party, List<Item> items)
        {
            List<CrawlerSaveItem> retval = new List<CrawlerSaveItem>();

            if (items == null)
            {
                return retval;
            }

            foreach (Item item in items)
            {
                CrawlerSaveItem newItem = new CrawlerSaveItem()
                {
                    Id = item.Id,
                    Name = item.Name,
                };

                if (string.IsNullOrEmpty(newItem.Id) || newItem.Id.Length > 6)
                {
                    newItem.Id = party.GetNextId("I");
                }

                newItem.Set(CIdx.ItemTypeId, item.ItemTypeId);
                newItem.Set(CIdx.LootRankId, item.LootRankId);
                newItem.Set(CIdx.Level, item.Level);
                newItem.Set(CIdx.ScalingTypeId, item.ScalingTypeId);
                newItem.Set(CIdx.EquipSlotId, item.EquipSlotId);
                newItem.Set(CIdx.BuyCost, item.BuyCost);
                newItem.Set(CIdx.SellValue, item.SellValue);
                newItem.Set(CIdx.QualityTypeId, item.QualityTypeId);

                newItem.Effects = new List<ItemEffect>(item.Effects);

                newItem.CreateDatString();
                retval.Add(newItem);
            }
            return retval;
        }

        private List<Item> ConvertItemsFromSaveToGame(PartyData party, List<CrawlerSaveItem> saveItems)
        {
            List<Item> retval = new List<Item>();
            if (saveItems == null)
            {
                return retval;
            }

            foreach (CrawlerSaveItem saveItem in saveItems)
            {
                Item newItem = new Item()
                {
                    Id = saveItem.Id,
                    Name = saveItem.Name,
                    BuyCost = saveItem.Get(CIdx.BuyCost),
                    ScalingTypeId = saveItem.Get(CIdx.ScalingTypeId),
                    EquipSlotId = saveItem.Get(CIdx.EquipSlotId),
                    ItemTypeId = saveItem.Get(CIdx.ItemTypeId),
                    Level = (int)saveItem.Get(CIdx.Level),
                    LootRankId = saveItem.Get(CIdx.LootRankId),
                    QualityTypeId = saveItem.Get(CIdx.QualityTypeId),
                    Quantity = 1,
                    SellValue = saveItem.Get(CIdx.SellValue),
                    Procs = new List<ItemProc>()
                };

                newItem.Effects.AddRange(saveItem.Effects);

                retval.Add(newItem);

            }
            return retval;
        }

        public void InitPartyAfterLoad(PartyData party)
        {
            _awaitableService.ForgetAwaitable(StartGameAfterLoadAsync(party));
        }


        private async Awaitable StartGameAfterLoadAsync(PartyData party)
        {
            if (party == null)
            {
                return;
            }


            _dispatcher.Dispatch(new CloseAllScreens() { KeepOpenScreens = new List<long>() { ScreenNames.Loading } });
            await _screenService.OpenAsync(ScreenNames.Loading, null, GetToken());

            _party = party;
            _party.Inventory = ConvertItemsFromSaveToGame(_party, _party.SaveInventory);

            // Party.Members is only for backwards compat with older savefiles.
            if (_party.Members != null && _party.Members.Count > 0)
            {
                foreach (PartyMember member in _party.Members)
                {
                    if (member.PartySlot > 0)
                    {
                        _party.ActiveParty.Add(member);
                    }
                    else
                    {
                        _party.InGuild.Add(member);
                    }
                }
                _party.Members.Clear();
            }

            foreach (PartyMember member in _party.GetAllMembers())
            {
                member.Equipment = ConvertItemsFromSaveToGame(_party, member.SaveEquipment);
                member.ConvertDataAfterLoad();
            }


            party.ActiveParty = party.ActiveParty.OrderBy(x => x.PartySlot).ToList();
            foreach (PartyMember member in party.ActiveParty)
            {
                _spellService.SetupCombatData(party, member);
            }

            _crawlerStatService.CalcPartyStats(_party, false);
            _inputService.SetDisabled(true);

            if (party.WorldId < 1)
            {
                party.WorldId = _rand.Next() % 5000000;
            }

            CrawlerWorld world = await _worldService.GetWorld(_party.WorldId);

            await _screenService.OpenAsync(GetCrawlerScreenId(), null, _token);

            if (party.HasFlag(PartyFlags.InGuildHall) || party.ActiveParty.Count < 1)
            {
                ChangeState(ECrawlerStates.GuildMain, GetToken());
            }
            else
            {
                ChangeState(ECrawlerStates.ExploreWorld, GetToken());
            }

            while (_screenService.GetScreen(ScreenNames.NewCrawlerGame) != null)
            {
                await Awaitable.NextFrameAsync(_token);
            }

        }

        public bool ContinueGame()
        {
            PartyData party = _loadSaveService.ContinueGame<PartyData>();
            if (party != null)
            {
                InitPartyAfterLoad(party);
            }
            return party != null;
        }


        private PartyData LoadPremadeParty(long slot)
        {
            TextAsset textAsset = _localLoadService.LocalLoad<TextAsset>("Config/PartyDataPartyData1");
            PartyData party = null;
            if (textAsset != null)
            {
                party = _textSerializer.Deserialize<PartyData>(textAsset.text);
                if (party != null)
                {
                    return party;
                }
            }

            party = new PartyData() { Id = typeof(PartyData).Name + slot, SaveSlotId = slot, Seed = _rand.Next() };

            return party;
        }

        public PartyData LoadParty(long slot = 0)
        {
            PartyData party = _loadSaveService.LoadSlot<PartyData>(slot);

            if (party == null)
            {
                return null;
            }
            return party;
        }

        public void ClearAllStates()
        {
            _stateStack.Clear();
        }


        public async Task SaveGame()
        {
            if (_party != null)
            {

                if (_party.Combat != null)
                {
                    return;
                }

                _party.SaveInventory = ConvertItemsFromGameToSave(_party, _party.Inventory);

                foreach (PartyMember member in _party.GetAllMembers())
                {
                    member.SaveEquipment = ConvertItemsFromGameToSave(_party, member.Equipment);
                    member.ConvertDataBeforeSave();
                }

                _loadSaveService.Save(_party, _party.SaveSlotId, true);
            }
            await Task.CompletedTask;
        }


        private bool _canUpdateInputs = false;
        private void UpdateGame(CancellationToken token)
        {
            _canUpdateInputs = false;
            if (_stateQueue.Count > 0)
            {
                if (_stateQueue.Any(x => x.Action != null && x.Action.NextState != ECrawlerStates.DoNotChangeState))
                {
                    return;
                }
            }
            _canUpdateInputs = true;
        }

        public void OnKeyPress(Key key)
        {
            if (!_canUpdateInputs)
            {
                return;
            }
            if (_stateStack.TryPeek(out CrawlerStateData currentData))
            {
                bool shouldDispatchClickKeys = ShouldDispatchClickKeys();
                if (currentData.Actions.Count > 0)
                {
                    CrawlerStateAction action = currentData.Actions.FirstOrDefault(x => x.Key == key);
                    if (action != null)
                    {
                        //Explcitly set Escape to go back up a level, Do not have a global escape
                        // Also we do not check ALL keys every frame, just ones that the underlying state allows.
                        if (_inputService.WasPressedThisFrame(action.Key, shouldDispatchClickKeys))
                        {
                            if (action.NextState != ECrawlerStates.None)
                            {
                                ChangeState(currentData, action, _token);
                            }
                            else if (action.OnClickAction != null)
                            {
                                action.OnClickAction();
                            }
                        }
                    }
                }

                if (currentData.ShouldCheckInput() &&
                    (_inputService.WasPressedThisFrame(Key.Enter)))
                {
                    currentData.CheckInput();
                }
            }

            if (_inputService.ContinueKeyIsDown())
            {
                _shouldTriggerSpeedup = true;
            }
        }

        private bool _shouldTriggerSpeedup = false;
        public void ClearSpeedup()
        {
            _shouldTriggerSpeedup = false;
        }

        public bool TriggerSpeedupNow()
        {
            if (_shouldTriggerSpeedup)
            {
                _shouldTriggerSpeedup = false;
                return true;
            }
            return false;
        }
        public async Awaitable NewGame(int options)
        {
            _dispatcher.Dispatch(new OpenScreen(ScreenNames.Loading));

            _party = new PartyData();
            _party.Options = options;

            if (_optionsService.HasOption(_party, CrawlerOptions.WholeParty))
            {
                _party = LoadPremadeParty(LoadSaveConstants.MinSlot);
            }
            _party.Options = options;
            _party.Seed = _rand.Next();

            _party.Flags = 0;
            _party.DaysPlayed = 0;
            _party.HourOfDay = 0;
            IReadOnlyList<CrawlerCurrencyType> ctypes = _gameData.Get<CrawlerCurrencySettings>(_gs.ch).GetData();

            foreach (CrawlerCurrencyType ctype in ctypes)
            {
                _partyService.SetCurrencyQuantity(_party, ctype.IdKey, 0);
            }

            _partyService.SetCurrencyQuantity(_party, CrawlerCurrencyTypes.Gold, _gameData.Get<CrawlerSettings>(_gs.ch).StartGold);

            CrawlerSpellSettings spellSettings = _gameData.Get<CrawlerSpellSettings>(_gs.ch);

            _party.NextId = 1;
            foreach (PartyMember member in _party.GetAllMembers())
            {
                member.Exp = 0;
                member.Level = 1;
                foreach (UnitRole unitRole in member.Roles)
                {
                    unitRole.Level = 1;
                }
                member.Summons = new List<PartySummon>();
            }

            await StartGameAfterLoadAsync(_party);

            await _worldService.GenerateWorld(_party);

            //await _screenService.OpenAsync(ScreenNames.NewCrawlerGame, null, _token);
            _dispatcher.Dispatch(new CloseAllScreens());
            //_dispatcher.Dispatch(new CloseScreen(ScreenNames.Loading));



            ChangeState(ECrawlerStates.GuildMain, GetToken());
        }

        public List<IStateHelper> GetAllStateHelpers()
        {
            return _stateHelpers.GetDict().Values.ToList();
        }

        public ECrawlerStates GetPrevState(ECrawlerStates tryPrevState = ECrawlerStates.None)
        {
            if (tryPrevState != ECrawlerStates.None && _stateStack.Any(x => x.Id == tryPrevState))
            {
                return tryPrevState;
            }
            return GetTopLevelState().Id;
        }

        public void OnQuit()
        {
            SaveGame().Wait();
        }

        public void OnKeyRelease(Key key)
        {
        }
    }
}


