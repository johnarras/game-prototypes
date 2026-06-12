using MessagePack;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.Interfaces;
using System;
using System.Threading;

namespace OxDb.SharedGame.Users.Loaders
{
    public class SharedUserDataLoader<TServer> : ISharedUserDataLoader where TServer : class, IStringId, new()
    {
        private IRepositoryService _repoService = null;

        [IgnoreMember] public Type HelperKey => typeof(TServer);

        public async System.Threading.Tasks.Task CreateDefaultData(string userId)
        {
            TServer obj = await _repoService.Load<TServer>(userId);

            if (obj == null)
            {
                obj = new TServer() { Id = userId };
                await _repoService.Save(obj);
            }
        }

        public async System.Threading.Tasks.Task Initialize(CancellationToken token)
        {
            await System.Threading.Tasks.Task.CompletedTask;
        }
    }
}


