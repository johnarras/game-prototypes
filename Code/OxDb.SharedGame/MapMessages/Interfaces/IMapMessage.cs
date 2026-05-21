using System;

namespace OxDb.SharedGame.MapMessages.Interfaces
{
    public interface IMapMessage
    {
        DateTime GetLastExecuteTime();
        void SetLastExecuteTime(DateTime lastExecuteTime);
        bool IsCancelled();
        void SetCancelled(bool val);
    }
}


