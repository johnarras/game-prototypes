using OxDb.SharedCore.DataStores.Indexes;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.DataStores.Categories.PlayerData.ParentChild;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.Units.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Units.Loaders
{
    /// <summary>
    /// Class to load and save a list of items that can be individually loaded and saved
    /// </summary>
    /// <typeparam name="TParent">Parent container object (Think InventoryData)</typeparam>
    /// <typeparam name="TChild">Child type object (think Items)</typeparam>
    /// <typeparam name="TDto">Type used to send the parent data to the client (since the Parent has no public list.</typeparam>
    public abstract class OwnerDataLoader<TParent, TChild> : UnitDataLoader<TParent>
        where TParent : OwnerObjectList<TChild>, ISearchableItem, new()
        where TChild : OwnerPlayerData, ISearchableItem, IChildUnitData
    {

        public override List<CreateIndexData> GetIndexes()
        {
            CreateIndexData cid = new CreateIndexData(typeof(TChild));
            cid.Configs.Add(new IndexConfig() { MemberName = nameof(OwnerPlayerData.OwnerId) });

            return new List<CreateIndexData>() { cid };
        }
        public override async System.Threading.Tasks.Task Initialize(CancellationToken token)
        {
            await System.Threading.Tasks.Task.CompletedTask;
        }

        public override async Task<ITopLevelUnitData> LoadFullData(Unit unit)
        {
            string id = unit.Id;

            if (IsUserData() && unit is Character ch)
            {
                id = ch.UserId;
            }

            Task<TParent> parentTask = _repoService.Load<TParent>(id);
            Task<List<TChild>> childTask = _repoService.Search<TChild>(x => x.OwnerId == id);

            await System.Threading.Tasks.Task.WhenAll(parentTask, childTask).ConfigureAwait(false);

            TParent parent = await parentTask;
            List<TChild> items = await childTask;
            if (parent != null)
            {
                parent.SetData(items);
            }
            return parent;
        }

    }
}


