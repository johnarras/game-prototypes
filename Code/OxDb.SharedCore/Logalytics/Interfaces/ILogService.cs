using OxDb.SharedCore.Interfaces;
using System;

namespace OxDb.SharedCore.Logalytics.Interfaces
{
    public interface ILogService : IPriorityInitializable, IExplicitInject
    {
        void Verbose(string txt);
        void Info(string txt);
        void Warning(string txt);
        void Debug(string txt);
        void Error(string txt);
        void Exception(Exception e, string txt);
    }
}


