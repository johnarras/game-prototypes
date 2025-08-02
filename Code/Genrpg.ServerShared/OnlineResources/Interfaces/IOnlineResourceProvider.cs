using Genrpg.ServerShared.CloudComms.Platforms;
using Genrpg.ServerShared.CloudComms.Services;
using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.DataStores.Entities;
using Genrpg.ServerShared.Secrets.Services;
using Genrpg.Shared.DataStores.DataGroups;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Tasks.Services;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.OnlineResources.Interfaces
{
    public interface IOnlineResourceProvider : IInjectable
    {
        public Task<IRepository> CreateRepo(InitRepoArgs args);
        public Task<ICloudMessageImpl> CreateCloudMessageImpl(IServiceLocator loc, 
            IServerConfig config, ILogService logService, ITextSerializer serializer, ISecretsProvider secretsProvider,
            ITaskService taskService,
            ICloudCommsService cloudCommsService, CancellationToken token);

        public string GetPublicIPAddress(IServerConfig config, ILogService logService, ISecretsProvider secretsProvider, CancellationToken token);
    }
}
