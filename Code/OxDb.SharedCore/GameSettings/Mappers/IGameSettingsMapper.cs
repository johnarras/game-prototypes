using OxDb.SharedCore.GameSettings.Interfaces;
using OxDb.SharedCore.Interfaces;
using System;

namespace OxDb.SharedCore.GameSettings.Mappers
{
    /// <summary>
    /// Use for mapping between client and server. Split from loader so client<->server and server<->database can vary independently
    /// </summary>
    public interface IGameSettingsMapper : ISetupDictionaryItem<Type>
    {
        Version GetMinClientVersion();
        Version GetMaxClientVersion();
        Type GetClientType();
        bool SendToClient();
        ITopLevelSettings MapToDto(ITopLevelSettings settings, bool simplify);
    }
}


