using Genrpg.Shared.Interfaces;
using System;

namespace Genrpg.Shared.Logging.Interfaces
{
    public interface ILogService : IPriorityInitializable, IExplicitInject
    {
        void Message(string txt);
        void Info(string txt);
        void Warning(string txt);
        void Debug(string txt);
        void Error(string txt);
        void Exception(Exception e, string txt);
    }
}


