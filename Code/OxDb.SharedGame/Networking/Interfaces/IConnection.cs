using OxDb.SharedGame.MapMessages.Interfaces;
using OxDb.SharedGame.Networking.Messages;
using System;

namespace OxDb.SharedGame.Networking.Interfaces
{
    public interface IConnection
    {
        void ForceClose();
        void AddMessage(IMapApiMessage message);
        bool RemoveMe();
        ConnMessageCounts GetCounts();
        void Shutdown(Exception e, string message);
        void SendError(string message);
    }
}


