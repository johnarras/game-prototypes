using Genrpg.Shared.DataStores.Categories.PlayerData.NoChild;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.DataStores.Constants;
using Genrpg.Shared.DataStores.Interfaces;
using MessagePack;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Units.Mappers
{
    public class NoChildUnitDataMapper<TPlayerData, TDto> : IUnitDataMapper where TPlayerData : NoChildPlayerData where TDto : NoChildPlayerDataDto<TPlayerData>, new()
    {

        public virtual Version GetMinClientVersion() { return VersionConstants.MinVersion; }
        public virtual Version GetMaxClientVersion() { return VersionConstants.MaxVersion; }
        public async Task Initialize(CancellationToken token)
        {
            await Task.CompletedTask;
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


