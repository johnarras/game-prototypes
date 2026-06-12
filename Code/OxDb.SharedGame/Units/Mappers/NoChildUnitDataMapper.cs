using MessagePack;
using OxDb.SharedCore.DataStores.Constants;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedGame.DataStores.Categories.PlayerData.NoChild;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using System;
using System.Threading;

namespace OxDb.SharedGame.Units.Mappers
{
    public class NoChildUnitDataMapper<TPlayerData, TDto> : IUnitDataMapper where TPlayerData : NoChildPlayerData where TDto : NoChildPlayerDataDto<TPlayerData>, new()
    {

        public virtual Version GetMinClientVersion() { return VersionConstants.MinVersion; }
        public virtual Version GetMaxClientVersion() { return VersionConstants.MaxVersion; }
        public async System.Threading.Tasks.Task Initialize(CancellationToken token)
        {
            await System.Threading.Tasks.Task.CompletedTask;
        }
        public virtual IUnitData MapToAPI(IUnitData serverObject)
        {
            TPlayerData playerData = serverObject as TPlayerData;
            TDto dto = new TDto();
            dto.Parent = playerData;
            return dto;
        }

        public bool SendToClient()
        {
            return !typeof(IServerOnlyData).IsAssignableFrom(HelperKey);
        }

        [IgnoreMember] public virtual Type HelperKey => typeof(TPlayerData);
    }
}


