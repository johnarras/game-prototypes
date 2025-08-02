using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.Website.Messages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.GameSettings.Services;

public interface IGameDataService : IInjectable
{
    Task<IGameData> LoadGameData(bool createMissingGameData);
    Task ReloadGameData();
    Task<bool> SaveGameData(IGameData data, IRepositoryService repoSystem);
    List<string> GetEditorIgnoreFields();
    List<IGameSettingsLoader> GetAllLoaders();
    Dictionary<Type, IGameSettingsMapper> GetAllMappers();
    bool AcceptedByFilter(IFilteredObject obj, IPlayerFilter filter);
    List<ITopLevelSettings> MapToDto(IFilteredObject obj, List<ITopLevelSettings> startSettings);
    bool SetGameDataOverrides(IFilteredObject fobj, bool forceRefresh);
    void GetClientSettings(WebResponseList list, IFilteredObject fobj, bool forceUpdateOverrides);
}
