using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.Trades.Messages;
using System;

namespace OxDb.MapServer.Trades.Services
{
    public interface ITradeService : IInjectable
    {
        void HandleStartTrade(Character ch, StartTrade startTrade);
        void HandleCancelTrade(Character ch, CancelTrade cancelTrade);
        void HandleOnCancelTrade(Character ch, OnCancelTrade message);
        void HandleAcceptTrade(Character ch, AcceptTrade acceptTrade, IRandom rand);
        void HandleOnAcceptTrade(Character ch, OnAcceptTrade message);
        void HandleUpdateTrade(Character ch, UpdateTrade updateTrade);
        void HandleOnUpdateTrade(Character ch, OnUpdateTrade message);
        void HandleOnCompleteTrade(Character ch, OnCompleteTrade message);
        T SafeModifyObject<T>(MapObject obj, Func<T> modifyFunc, T failureResult);
        void SafeModifyObject(MapObject obj, Action modifyFunc);
    }
}


