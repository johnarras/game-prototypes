using OxDb.SharedCore.DataStores.Indexes;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.DataStores.Categories.PlayerData.ParentChild;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Units.Loaders
{
    public class OwnerIdDataLoader<TParent, TChild> : OwnerDataLoader<TParent, TChild>
        where TParent : OwnerObjectList<TChild>, new()
        where TChild : OwnerPlayerData, IChildUnitData, IId
    {


        public override List<CreateIndexData> GetIndexes()
        {
            CreateIndexData cid = new CreateIndexData(typeof(TChild));
            cid.Configs.Add(new IndexConfig() { MemberName = nameof(OwnerPlayerData.OwnerId), CompoundContinue = true });
            cid.Configs.Add(new IndexConfig() { MemberName = nameof(IId.IdKey) });

            return new List<CreateIndexData> { cid };
        }

        public override async Task Initialize(CancellationToken token)
        {
            await base.Initialize(token);
        }

    }
}


