
using Assets.Scripts.Assets;
using Assets.Scripts.Awaitables;
using Assets.Scripts.ClientEvents;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.UI.Constants;
using Assets.Scripts.UI.Entities;
using Assets.Scripts.UI.Interfaces;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Combat.Services;
using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Crawler.Items.Entities;
using Genrpg.Shared.Crawler.Loot.Services;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Party.Services;
using Genrpg.Shared.Crawler.Spells.Services;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Crawler.States.StateHelpers;
using Genrpg.Shared.Crawler.Stats.Services;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.LoadSave.Constants;
using Genrpg.Shared.LoadSave.Services;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.UI.Constants;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

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
        private IPartyService _partyService = null;

        public const string SaveFileSuffix = ".sav";
        public const string StartSaveFileName = "Start" + SaveFileSuffix;

        private SetupDictionaryContainer<ECrawlerStates, IStateHelper> _stateHelpers = new SetupDictionaryContainer<ECrawlerStates, IStateHelper>();

        private PartyData _party { get; set; }

        public PartyData GetParty()
        {
            return _party;
        }

        private Stack<CrawlerStateData> _stateStack { get; set; } = new Stack<CrawlerStateData>();


        private Dictionary<char, char> _equivalentKeys = new Dictionary<char, char>();

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
            CrawlerStateAction action = new CrawlerStateAction(null, CharCodes.None, crawlerState, extraData: extraData);
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
            if (_moveService.UpdatingMovement())
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

            if (_inputService.GetKeyDown(CharCodes.Escape))
            {
                ActiveScreen activeScreen = _screenService.GetLayerScreen(ScreenLayers.Screens);
                if (activeScreen != null)
                {
                    _screenService.Close(activeScreen.ScreenId);
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

        private async Awaitable ChangeStateAsync(FullCrawlerState fullState, CancellationToken token)
        {
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
                    //_logService.Info("ChangeState: " + stateHelper.GetKey().ToString());
                    nextStateData = await stateHelper.Init(currData, action, token);

                    if (nextStateData.DoNotTransitionToThisState)
                    {
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

        private void InitPartyAfterLoad(PartyData party, bool newGame)
        {
            _awaitableService.ForgetAwaitable(InitPartyAfterLoadAsync(party, newGame));
        }

        private async Awaitable InitPartyAfterLoadAsync(PartyData party, bool newGame)
        {
            if (party == null)
            {
                return;
            }

            if (newGame)
            {
                await _screenService.OpenAsync(ScreenNames.NewCrawlerGame, null, _token);
                _screenService.Close(ScreenNames.Loading);
            }
            else
            {
                _screenService.CloseAll(new List<long>() { ScreenNames.Loading });
                await _screenService.OpenAsync(ScreenNames.Loading, null, GetToken());
            }

            _party = party;
            _party.Inventory = ConvertItemsFromSaveToGame(_party, _party.SaveInventory);

            foreach (PartyMember member in _party.Members)
            {
                member.Equipment = ConvertItemsFromSaveToGame(_party, member.SaveEquipment);
                member.ConvertDataAfterLoad();
            }

            foreach (PartyMember member in party.GetActiveParty())
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

            if (party.HasFlag(PartyFlags.InGuildHall) || party.GetActiveParty().Count < 1)
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
            InitPartyAfterLoad(party, false);
            return party != null;
        }


        private PartyData CreatePartyDataForSlot(long slot)
        {
            TextAsset textAsset = _localLoadService.LocalLoad<TextAsset>("Config/PartyDataPartyData1");
            if (textAsset != null)
            {
                PartyData partyData = _textSerializer.Deserialize<PartyData>(textAsset.text);
                if (partyData != null)
                {
                    return partyData;
                }
            }
            PartyData party = new PartyData() { Id = typeof(PartyData).Name + slot, SaveSlotId = slot, Seed = _rand.Next() };

            _partyService.AddGold(party, 1000);
            return party;
        }

        public PartyData LoadParty(long slot = 0)
        {
            PartyData party = _loadSaveService.LoadSlot<PartyData>(slot);

            if (party == null)
            {
                return null;
            }

            InitPartyAfterLoad(party, false);

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

                foreach (PartyMember member in _party.Members)
                {
                    member.SaveEquipment = ConvertItemsFromGameToSave(_party, member.Equipment);
                    member.ConvertDataBeforeSave();
                }

                _loadSaveService.Save(_party, _party.SaveSlotId, true);
            }
            await Task.CompletedTask;
        }

        private void UpdateGame(CancellationToken token)
        {
            if (_stateQueue.Count > 0)
            {
                if (_stateQueue.Any(x => x.Action != null && x.Action.NextState != ECrawlerStates.DoNotChangeState))
                {
                    return;
                }
            }

            UpdateInputs(token);
        }

        public void UpdateInputs(CancellationToken token)
        {
            if (_stateStack.TryPeek(out CrawlerStateData currentData))
            {

                bool shouldDispatchClickKeys = ShouldDispatchClickKeys();
                if (currentData.Actions.Count > 0)
                {
                    foreach (CrawlerStateAction action in currentData.Actions)
                    {
                        //Explcitly set Escape to go back up a level, Do not have a global escape
                        // Also we do not check ALL keys every frame, just ones that the underlying state allows.
                        if (_inputService.GetKeyDown(action.Key, shouldDispatchClickKeys))
                        {
                            if (action.NextState != ECrawlerStates.None)
                            {
                                ChangeState(currentData, action, token);
                                break;
                            }
                        }
                        else if (_equivalentKeys.TryGetValue(action.Key, out char otherKey))
                        {
                            if (_inputService.GetKey(otherKey))
                            {
                                ChangeState(currentData, action, token);
                                break;
                            }
                        }

                    }
                }

                if (currentData.ShouldCheckInput() &&
                    (_inputService.GetKeyDown(CharCodes.Return) ||
                    _inputService.GetKeyDown(CharCodes.Enter)))
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
        public void NewGame()
        {
            _screenService.CloseAll();
            _screenService.Open(ScreenNames.Loading);
            PartyData party = CreatePartyDataForSlot(LoadSaveConstants.MinSlot);
            _party = party;
            InitPartyAfterLoad(party, true);
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
    }
}
