using OxDb.MapServer.MapMessaging.Interfaces;
using OxDb.MapServer.Maps;
using OxDb.ServerCore.DataStores.Services;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Currencies.Constants;
using OxDb.SharedGame.Currencies.PlayerData;
using OxDb.SharedGame.Inventory.PlayerData;
using OxDb.SharedGame.Inventory.Services;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.Rewards.Constants;
using OxDb.SharedGame.Rewards.Services;
using OxDb.SharedGame.Trades.Constants;
using OxDb.SharedGame.Trades.Entities;
using OxDb.SharedGame.Trades.Messages;
using OxDb.SharedGame.Units.Constants;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.MapServer.Trades.Services
{
    public interface ITradeService : IInjectable
    {
        ValueTask HandleStartTrade(Character ch, StartTrade startTrade);
        ValueTask HandleCancelTrade(Character ch, CancelTrade cancelTrade);
        void HandleOnCancelTrade(Character ch, OnCancelTrade message);
        ValueTask HandleAcceptTrade(Character ch, AcceptTrade acceptTrade);
        void HandleOnAcceptTrade(Character ch, OnAcceptTrade message);
        ValueTask HandleUpdateTrade(Character ch, UpdateTrade updateTrade);
        void HandleOnUpdateTrade(Character ch, OnUpdateTrade message);
        ValueTask HandleOnCompleteTrade(Character ch, OnCompleteTrade message);
        ValueTask<T> SafeModifyObjectAsync<T>(MapObject obj, Func<ValueTask<T>> modifyFunc, T failureResult);
        ValueTask SafeModifyObjectAsync(MapObject obj, Func<ValueTask> modifyFunc);
    }

    public class TradeService : ITradeService
    {
        private IMapObjectManager _objManager = null;
        private IMapMessageService _messageService = null;
        private IInventoryService _inventoryService = null;
        private IFullRepositoryService _repoService = null;
        private IRewardService _rewardService = null;

        // Assuming a shared lock object for global trade state coordination if needed, 
        // or using the individual character semaphores.
        private static readonly SemaphoreSlim _globalTradeStateLock = new SemaphoreSlim(1, 1);

        #region Utils
        private void SendError(Character ch, string message)
        {
            ch.SendError(message);
        }

        private async ValueTask ProcessExistingTrade(Character ch, Func<FullTradeObject, ValueTask> internalTradeAction)
        {
            FullTradeObject fullTrade = GetFullTradeObject(ch);

            if (!string.IsNullOrEmpty(fullTrade.ErrorMessage))
            {
                ch.AddMessage(new OnCancelTrade() { CharId = ch.Id, ErrorMessage = fullTrade.ErrorMessage });
                return;
            }

            // Await locks sequentially in a sorted, deterministic order to prevent deadlocks
            await _globalTradeStateLock.WaitAsync();
            try
            {
                await fullTrade.OrderedCharacters[0].TradeLock.WaitAsync();
                try
                {
                    await fullTrade.OrderedCharacters[1].TradeLock.WaitAsync();
                    try
                    {
                        if (!fullTrade.IsOkToUpdate())
                        {
                            foreach (Character tch in fullTrade.OrderedCharacters)
                            {
                                CancelCharTrade(tch);
                            }
                            return;
                        }

                        await internalTradeAction(fullTrade);
                    }
                    finally
                    {
                        fullTrade.OrderedCharacters[1].TradeLock.Release();
                    }
                }
                finally
                {
                    fullTrade.OrderedCharacters[0].TradeLock.Release();
                }
            }
            finally
            {
                _globalTradeStateLock.Release();
            }
        }

        private FullTradeObject GetFullTradeObject(Character ch)
        {
            FullTradeObject fullTrade = new FullTradeObject();

            if (ch.Trade == null)
            {
                fullTrade.ErrorMessage = "You're not trading";
                return fullTrade;
            }

            string otherId = ch.Id;
            bool foundMyCharId = false;
            for (int i = 0; i < ch.Trade.Chars.Length; i++)
            {
                if (ch.Trade.Chars[i].CharId != ch.Id)
                {
                    otherId = ch.Trade.Chars[i].CharId;
                }
                else
                {
                    foundMyCharId = true;
                }
            }

            if (!_objManager.GetChar(otherId, out Character ch2))
            {
                CancelCharTrade(ch);
                fullTrade.ErrorMessage = "Other character does not exist";
                return fullTrade;
            }

            if (!foundMyCharId)
            {
                CancelCharTrade(ch);
                CancelCharTrade(ch2);
                fullTrade.ErrorMessage = "You aren't in this trade.";
                return fullTrade;
            }

            fullTrade.OrderedCharacters.Add(ch);

            if (ch.Id.CompareTo(otherId) < 0)
            {
                fullTrade.OrderedCharacters.Add(ch2);
            }
            else
            {
                fullTrade.OrderedCharacters.Insert(0, ch2);
            }

            fullTrade.TradeObject = ch.Trade;
            return fullTrade;
        }
        #endregion

        #region Accept
        public async ValueTask HandleAcceptTrade(Character ch, AcceptTrade acceptTrade)
        {
            await ProcessExistingTrade(ch, async delegate (FullTradeObject fullTrade)
            {
                await HandleAcceptTradeInternal(ch, acceptTrade, fullTrade);
            });
        }

        private async ValueTask HandleAcceptTradeInternal(Character ch, AcceptTrade acceptTrade, FullTradeObject fullTrade)
        {
            foreach (TradeChar tch in fullTrade.TradeObject.Chars)
            {
                if (tch.CharId == ch.Id)
                {
                    tch.Accepted = true;
                    break;
                }
            }

            bool allAccepted = true;
            foreach (TradeChar tch in fullTrade.TradeObject.Chars)
            {
                if (!tch.Accepted)
                {
                    allAccepted = false;
                    break;
                }
            }

            foreach (Character tch in fullTrade.OrderedCharacters)
            {
                tch.AddMessage(new OnAcceptTrade() { CharId = ch.Id });
            }

            if (!allAccepted)
            {
                return;
            }

            // All accepted so complete.
            fullTrade.TradeObject.State = ETradeStates.Complete;

            foreach (Character tch in fullTrade.OrderedCharacters)
            {
                tch.Trade = null;
            }

            for (int c = 0; c < fullTrade.TradeObject.Chars.Length; c++)
            {
                TradeChar currTrade = fullTrade.TradeObject.Chars[c];
                Character currChar = fullTrade.OrderedCharacters[c];
                Character otherChar = fullTrade.OrderedCharacters[1 - c];

                // Safely await the reward updates natively without hacky synchronous stack checks
                bool removeSuccess = await _rewardService.GiveReward(currChar, EntityTypes.CharCurrency, CharCurrencyTypes.Money, -currTrade.Money, RewardSources.PlayerTrade, null, 0, null);
                if (!removeSuccess)
                {
                    throw new InvalidOperationException($"Trade completion failed! Unable to deduct currency for CharId {currChar.Id}.");
                }

                bool addSuccess = await _rewardService.GiveReward(otherChar, EntityTypes.CharCurrency, CharCurrencyTypes.Money, currTrade.Money, RewardSources.PlayerTrade, null, 0, null);
                if (!addSuccess)
                {
                    throw new InvalidOperationException($"Trade completion failed! Unable to add currency for CharId {otherChar.Id}.");
                }

                for (int i = 0; i < currTrade.Items.Length; i++)
                {
                    if (currTrade.Items[i] != null)
                    {
                        currTrade.Items[i].OwnerId = otherChar.Id;
                        _repoService.QueueSave(currTrade.Items[i]);
                    }
                }
            }

            foreach (Character tch in fullTrade.OrderedCharacters)
            {
                _messageService.SendMessage(tch, new OnCompleteTrade() { TradeObject = fullTrade.TradeObject });
            }
        }

        public void HandleOnAcceptTrade(Character ch, OnAcceptTrade onAcceptTrade)
        {
            ch.AddMessage(onAcceptTrade);
        }
        #endregion

        #region Cancel
        public async ValueTask HandleCancelTrade(Character ch, CancelTrade cancelTrade)
        {
            await ProcessExistingTrade(ch, (FullTradeObject fullTrade) =>            
                HandleCancelTradeInternal(ch, cancelTrade, fullTrade)
            );
        }

        private async ValueTask HandleCancelTradeInternal(Character ch, CancelTrade cancelTrade, FullTradeObject fullTrade)
        {
            foreach (Character tradeChar in fullTrade.OrderedCharacters)
            {
                CancelCharTrade(tradeChar);
            }
        }

        private void CancelCharTrade(Character ch)
        {
            if (ch.Trade != null)
            {
                ch.Trade.State = ETradeStates.Cancelled;
                ch.Trade = null;
            }
            ch.AddMessage(new OnCancelTrade());
        }

        public void HandleOnCancelTrade(Character ch, OnCancelTrade onCancelTrade)
        {
            ch.Trade = null;
            ch.AddMessage(onCancelTrade);
        }
        #endregion

        #region Start
        public async ValueTask HandleStartTrade(Character ch, StartTrade startTrade)
        {
            if (ch.Id == startTrade.CharId)
            {
                ch.SendError("You cannot trade with yourself.");
                return;
            }

            if (!_objManager.GetChar(startTrade.CharId, out Character ch2))
            {
                ch.SendError("That player does not exist.");
                return;
            }

            List<Character> orderedChars = new List<Character> { ch };
            if (string.Compare(ch.Id, startTrade.CharId) < 0)
            {
                orderedChars.Add(ch2);
            }
            else
            {
                orderedChars.Insert(0, ch2);
            }

            await orderedChars[0].TradeLock.WaitAsync();
            try
            {
                if (orderedChars[0].Trade != null)
                {
                    ch.SendError(orderedChars[0] == ch ? "You are already trading." : "They are already trading.");
                    return;
                }

                await orderedChars[1].TradeLock.WaitAsync();
                try
                {
                    if (orderedChars[1].Trade != null)
                    {
                        ch.SendError(orderedChars[1] == ch ? "You are already trading." : "They are already trading.");
                        return;
                    }

                    if (ch.FactionTypeId != ch2.FactionTypeId)
                    {
                        ch.SendError("You cannot trade with other factions");
                        return;
                    }

                    if (ch.HasFlag(UnitFlags.IsDead))
                    {
                        ch.SendError("You are dead");
                        return;
                    }

                    if (ch2.HasFlag(UnitFlags.IsDead))
                    {
                        ch.SendError("They are dead");
                        return;
                    }

                    TradeObject tradeObject = new TradeObject();
                    for (int i = 0; i < orderedChars.Count; i++)
                    {
                        tradeObject.Chars[i].CharId = orderedChars[i].Id;
                        tradeObject.Chars[i].CharName = orderedChars[i].Name;
                        orderedChars[i].Trade = tradeObject;
                    }

                    OnUpdateTrade onUpdateTrade = new OnUpdateTrade() { TradeObject = tradeObject };

                    for (int i = 0; i < orderedChars.Count; i++)
                    {
                        Character currChar = orderedChars[i];
                        Character otherChar = orderedChars[1 - i];
                        currChar.AddMessage(new OnStartTrade() { CharId = otherChar.Id, Name = otherChar.Name });
                    }
                }
                finally
                {
                    orderedChars[1].TradeLock.Release();
                }
            }
            finally
            {
                orderedChars[0].TradeLock.Release();
            }
        }
        #endregion

        #region SafeModifyObject
        public async ValueTask<T> SafeModifyObjectAsync<T>(MapObject obj, Func<ValueTask<T>> modifyFunc, T failureResult)
        {
            if (obj is Character ch)
            {
                if (Interlocked.Read(ref ch.TradeModifyLockCount) > 0)
                {
                    return await modifyFunc();
                }
                else
                {
                    await ch.TradeLock.WaitAsync();
                    try
                    {
                        Interlocked.Increment(ref ch.TradeModifyLockCount);

                        if (ch.Trade != null)
                        {
                            ch.SendError("You are trading.");
                            return failureResult;
                        }

                        T returnVal = await modifyFunc();
                        Interlocked.Decrement(ref ch.TradeModifyLockCount);
                        return returnVal;
                    }
                    finally
                    {
                        ch.TradeLock.Release();
                    }
                }
            }
            else
            {
                return await modifyFunc();
            }
        }

        public async ValueTask SafeModifyObjectAsync(MapObject obj, Func<ValueTask> modifyFunc)
        {
            await SafeModifyObjectAsync<bool>(obj, async () => { await modifyFunc(); return true; }, false);
        }
        #endregion

        #region Update
        public async ValueTask HandleUpdateTrade(Character ch, UpdateTrade updateTrade)
        {
            await ProcessExistingTrade(ch, async delegate (FullTradeObject fullTrade)
            {
                await HandleUpdateTradeInternal(ch, updateTrade, fullTrade);
            });
        }

        private async ValueTask HandleUpdateTradeInternal(Character ch, UpdateTrade updateTrade, FullTradeObject fullTrade)
        {
            TradeChar tradeChar = null;
            for (int i = 0; i < fullTrade.TradeObject.Chars.Length; i++)
            {
                if (fullTrade.TradeObject.Chars[i].CharId == ch.Id)
                {
                    tradeChar = fullTrade.TradeObject.Chars[i];
                    break;
                }
            }

            if (tradeChar == null)
            {
                ch.SendError("You aren't in this trade.");
                return;
            }

            long charMoney = ch.Get<CharCurrencyData>().Data[CharCurrencyTypes.Money];

            if (charMoney < updateTrade.Money)
            {
                await HandleCancelTradeInternal(ch, new CancelTrade() { CharId = ch.Id }, fullTrade);
                ch.SendError("You don't have enough money.");
                return;
            }

            InventoryData inventoryData = ch.Get<InventoryData>();
            Item[] newItems = new Item[TradeConstants.MaxItems];
            List<string> itemIds = new List<string>();

            for (int i = 0; i < updateTrade.ItemIds.Length; i++)
            {
                if (string.IsNullOrEmpty(updateTrade.ItemIds[i]))
                {
                    continue;
                }

                if (itemIds.Contains(updateTrade.ItemIds[i]))
                {
                    await HandleCancelTradeInternal(ch, new CancelTrade() { CharId = ch.Id }, fullTrade);
                    ch.SendError("The same item is in the trade twice.");
                    return;
                }

                Item myItem = inventoryData.GetItem(updateTrade.ItemIds[i]);
                if (myItem == null)
                {
                    await HandleCancelTradeInternal(ch, new CancelTrade() { CharId = ch.Id }, fullTrade);
                    ch.SendError("You are missing an item.");
                    return;
                }
                newItems[i] = myItem;
                itemIds.Add(updateTrade.ItemIds[i]);
            }

            tradeChar.Money = updateTrade.Money;
            tradeChar.Items = newItems;

            foreach (TradeChar tradeChar2 in fullTrade.TradeObject.Chars)
            {
                tradeChar2.Accepted = false;
            }

            foreach (Character ch2 in fullTrade.OrderedCharacters)
            {
                ch2.AddMessage(new OnUpdateTrade() { TradeObject = fullTrade.TradeObject });
            }
        }

        public void HandleOnUpdateTrade(Character ch, OnUpdateTrade onUpdateTrade)
        {
            ch.AddMessage(onUpdateTrade);
        }
        #endregion

        #region Complete
        public async ValueTask HandleOnCompleteTrade(Character ch, OnCompleteTrade onCompleteTrade)
        {
            ch.Trade = null;
            await SafeModifyObjectAsync(ch, () => SafeHandleOnCompleteTrade(ch, onCompleteTrade));
                          
            ch.AddMessage(onCompleteTrade);
        }

        private async ValueTask SafeHandleOnCompleteTrade(Character ch, OnCompleteTrade onCompleteTrade)
        {
            foreach (TradeChar tradeChar in onCompleteTrade.TradeObject.Chars)
            {
                if (tradeChar.CharId == ch.Id)
                {
                    for (int i = 0; i < tradeChar.Items.Length; i++)
                    {
                        if (tradeChar.Items[i] != null)
                        {
                            await _inventoryService.RemoveItem(ch, tradeChar.Items[i].Id, false);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < tradeChar.Items.Length; i++)
                    {
                        if (tradeChar.Items[i] != null)
                        {
                            await _inventoryService.AddItem(ch, tradeChar.Items[i], true);
                        }
                    }
                }
            }
        }
        #endregion
    }
}