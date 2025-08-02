using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.Constants;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings.Interfaces;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.GameSettings.Loaders
{
    public abstract class NoChildSettingsLoader<TServer> : IGameSettingsLoader where TServer : NoChildSettings, new()
    {
        public virtual Type GetChildType() { return typeof(TServer); }
        public virtual bool SendToClient() { return true; }
        [IgnoreMember] public virtual Type Key => typeof(TServer); 
        public virtual async Task Initialize(CancellationToken token) { await Task.CompletedTask; }

        public virtual async Task<List<ITopLevelSettings>> LoadAll(IRepositoryService repoSystem, bool createDefaultIfMissing)
        {

            List<ITopLevelSettings> list = (await repoSystem.Search<TServer>(x => true)).Cast<ITopLevelSettings>().ToList();

            ITopLevelSettings defaultItem = list.FirstOrDefault(x => x.Id == GameDataConstants.DefaultFilename);

            if (defaultItem == null)
            {

                if (createDefaultIfMissing)
                {
                    list.Add(new TServer() { Id = GameDataConstants.DefaultFilename });
                }
                else
                {
                    throw new Exception("Missing NoChildSettings: " + typeof(TServer).FullName);    
                }
            }
         
            return list;
        }
    }
}
