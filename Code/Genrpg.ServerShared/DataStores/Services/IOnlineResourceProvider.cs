using Genrpg.ServerShared.CloudComms.Platforms;
using Genrpg.ServerShared.CloudComms.Services;
using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.DataStores.Entities;
using Genrpg.ServerShared.Secrets.Services;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Serialization.Interfaces;
using Genrpg.Shared.Tasks.Services;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.DataStores.Services
{
    public interface IOnlineResourceProvider : IInjectable
    {
        public Task<IRepository> CreateRepo(InitRepoArgs args, CancellationToken token);
        public Task<ICloudMessageImpl> CreateCloudMessageImpl(IServiceLocator loc,
            IServerConfig config, ILogService logService, ITextSerializer serializer, ISecretsProvider secretsProvider,
            ITaskService taskService,
            ICloudCommsService cloudCommsService, CancellationToken token);

        public string GetPublicIPAddress(IServerConfig config, ILogService logService, ISecretsProvider secretsProvider, CancellationToken token);
    }
}


