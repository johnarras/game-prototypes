using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Users.Loaders
{
    public class SharedUserDataLoader<TServer> : ISharedUserDataLoader where TServer : class, IStringId, new()
    {
        private IRepositoryService _repoService = null;

        [IgnoreMember] public Type Key => typeof(TServer);

        public async Task CreateDefaultData(string userId)
        {
            TServer obj = await _repoService.Load<TServer>(userId);

            if (obj == null)
            {
                obj = new TServer() { Id = userId };
                await _repoService.Save(obj);   
            }
        }

        public async Task Initialize(CancellationToken token)
        {
            await Task.CompletedTask;
        }
    }
}
