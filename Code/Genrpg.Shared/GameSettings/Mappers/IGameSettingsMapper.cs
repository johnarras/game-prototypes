using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Interfaces;
using System;

namespace Genrpg.Shared.GameSettings.Mappers
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
        ITopLevelSettings MapToDto(ITopLevelSettings settings);      
    }
}
