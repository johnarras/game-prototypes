using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.DataStores.Categories.PlayerData.Users;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Indexes;
using Genrpg.Shared.Units.Entities;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Units.Loaders
{
    public class UnitDataLoader<TServer> : IUnitDataLoader where TServer : class, ITopLevelUnitData, new()
    {

        protected IRepositoryService _repoService = null;

        [IgnoreMember] public virtual Type HelperKey => typeof(TServer);
        public bool IsUserData() { return typeof(IUserData).IsAssignableFrom(typeof(TServer)); }
        public virtual bool IsClientOnlyData() { return false; }
        public virtual List<CreateIndexData> GetIndexes() { return new List<CreateIndexData>(); }
        public virtual async Task Initialize(CancellationToken token) { await Task.CompletedTask; }
        public Type GetServerType() { return typeof(TServer); }
        public IUnitData Create(Unit unit)
        {
            TServer t = Activator.CreateInstance<TServer>();
            t.Id = GetFileId(unit);
            return t;
        }

        protected virtual string GetFileId(Unit unit)
        {
            if (!IsUserData())
            {
                return unit.Id;
            }
            if (unit is Character ch)
            {
                return ch.UserId;
            }
            return unit.Id;
        }

        public virtual async Task<ITopLevelUnitData> LoadFullData(Unit unit)
        {
            ITopLevelUnitData tld = await _repoService.Load<TServer>(GetFileId(unit));
            return tld;
        }

        public async Task<ITopLevelUnitData> LoadTopLevelData(Unit unit)
        {

            TServer currServer = unit.Get<TServer>();

            if (currServer != null)
            {
                return currServer;
            }

            currServer = await _repoService.Load<TServer>(GetFileId(unit));

            if (currServer != null)
            {
                unit.Set(currServer);
            }

            return currServer;
        }
    }
}


