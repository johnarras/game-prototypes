using MessagePack;
using OxDb.SharedCore.DataStores.Indexes;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.SharedCore.GameSettings.Loaders
{
    public class ParentSettingsLoader<TParent, TChild> : IGameSettingsLoader
        where TParent : ParentSettings<TChild>, new()
        where TChild : ChildSettings, new()
    {

        protected IRepositoryService _repoService = null;

        [IgnoreMember]
        public virtual Type HelperKey => typeof(TParent);
        public virtual Type GetChildType() { return typeof(TChild); }

        public virtual List<CreateIndexData> GetIndexes()
        {
            return new List<CreateIndexData>();


            // For now remove due to changing to new single collection method for this.
            //CreateIndexData indexData = new CreateIndexData(typeof(TChild));
            //indexData.Configs.Add(new IndexConfig() { Ascending = true, MemberName = nameof(ChildSettings.ParentId) });

            //return new List<CreateIndexData>() { indexData };
        }

        public virtual async Task Initialize(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        public virtual async Task<List<ITopLevelSettings>> LoadAll(ISearchRepositoryService repoSystem, bool createDefaultIfMissing)
        {

            Task<List<TParent>> loadParentsTask = repoSystem.Search<TParent>(x => true);

            Task<List<TChild>> loadChildrenTask = repoSystem.Search<TChild>(x => true);

            await Task.WhenAll(loadParentsTask, loadChildrenTask).ConfigureAwait(false);

            List<TParent> parents = await loadParentsTask;
            List<TChild> allChildren = await loadChildrenTask;

            TParent defaultObject = parents.FirstOrDefault(x => x.Id == GameDataConstants.DefaultFilename);
            if (defaultObject == null)
            {
                if (createDefaultIfMissing)
                {
                    defaultObject = new TParent() { Id = GameDataConstants.DefaultFilename };
                    parents.Add(defaultObject);
                }
                else
                {
                    throw new Exception("Missing ParentObject: " + typeof(TParent).FullName);
                }
            }

            foreach (TParent parent in parents)
            {
                parent.SetData(allChildren.Where(x => x.ParentId == parent.Id).ToList());
            }

            return parents.Cast<ITopLevelSettings>().ToList();
        }
    }
}


