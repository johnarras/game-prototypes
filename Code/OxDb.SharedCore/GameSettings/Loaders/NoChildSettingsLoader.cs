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
    public abstract class NoChildSettingsLoader<TParent> : IGameSettingsLoader where TParent : NoChildSettings, new()
    {
        public virtual Type GetChildType() { return typeof(TParent); }
        public virtual bool SendToClient() { return true; }
        [IgnoreMember] public virtual Type HelperKey => typeof(TParent);

        public virtual List<CreateIndexData> GetIndexes() { return new List<CreateIndexData>(); }
        public virtual async Task Initialize(CancellationToken token) { await Task.CompletedTask; }


        public ITopLevelSettings CreateDefaultDocument()
        {
            return new TParent() { Id = GameDataConstants.DefaultFilename };
        }

        public virtual async Task<List<ITopLevelSettings>> LoadAll(ISearchRepositoryService repoSystem, bool createDefaultIfMissing)
        {

            List<ITopLevelSettings> list = (await repoSystem.Search<TParent>(x => true)).Cast<ITopLevelSettings>().ToList();

            ITopLevelSettings defaultItem = list.FirstOrDefault(x => x.Id == GameDataConstants.DefaultFilename);

            if (defaultItem == null)
            {

                if (createDefaultIfMissing)
                {
                    list.Add(CreateDefaultDocument());
                }
                else
                {
                    throw new Exception("Missing NoChildSettings: " + typeof(TParent).FullName);
                }
            }

            return list;
        }

        public virtual void SetParentChildData(List<ITopLevelSettings> parents, List<IChildSettings> children)
        {
        }
    }
}


