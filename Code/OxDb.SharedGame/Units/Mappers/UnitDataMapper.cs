using OxDb.SharedCore.DataStores.Constants;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using System;
using System.Threading;

namespace OxDb.SharedGame.Units.Mappers
{
    public class UnitDataMapper<TServer> : IUnitDataMapper
        where TServer : class, ITopLevelUnitData, new()
    {
        public virtual Version GetMinClientVersion() { return VersionConstants.MinVersion; }
        public virtual Version GetMaxClientVersion() { return VersionConstants.MaxVersion; }
        public async System.Threading.Tasks.Task Initialize(CancellationToken token)
        {
            await System.Threading.Tasks.Task.CompletedTask;
        }
        public virtual IUnitData MapToAPI(IUnitData serverObject)
        {
            return serverObject;
        }

        public bool SendToClient()
        {
            return !typeof(IServerOnlyData).IsAssignableFrom(typeof(TServer));
        }

        public virtual Type HelperKey => typeof(TServer);

    }
}


