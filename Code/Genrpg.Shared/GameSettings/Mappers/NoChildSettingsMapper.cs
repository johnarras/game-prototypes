using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.Constants;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings.Interfaces;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Shared.GameSettings.Mappers
{
    public abstract class NoChildSettingsMapper<TServer,TDto> : IGameSettingsMapper where TServer : NoChildSettings, new()
        where TDto : NoChildSettingsDto<TServer>, new()
    {
        public virtual Version GetMinClientVersion() { return VersionConstants.MinVersion; }
        public virtual Version GetMaxClientVersion() { return VersionConstants.MaxVersion; }
        [IgnoreMember] public virtual Type Key => typeof(TServer);
        public virtual Type GetClientType() { return typeof(TDto); }
        public virtual bool SendToClient() { return true; }

        public virtual ITopLevelSettings MapToDto(ITopLevelSettings settings)
        {
            TDto dto = new TDto();
            TServer server = settings as TServer;
            dto.Parent = server;
            return dto;
        }

    }
}
