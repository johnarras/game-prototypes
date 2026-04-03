using Genrpg.ServerShared.CloudComms.Platforms;
using Genrpg.ServerShared.CloudComms.Services;
using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.DataStores.Entities;
using Genrpg.ServerShared.Secrets.Services;
using Genrpg.Shared.DataStores.DataGroups;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Serialization.Interfaces;
using Genrpg.Shared.Tasks.Services;
using Genrpg.Shared.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.DataStores.Services
{

    public class AzureOnlineResourceProvider : IOnlineResourceProvider
    {
        private ILogService _logService = null;

        private SetupDictionaryContainer<ERepoTypes, IAzureRepositoryProvider> _providers = new SetupDictionaryContainer<ERepoTypes, IAzureRepositoryProvider>();

        public async Task<ICloudMessageImpl> CreateCloudMessageImpl(IServiceLocator loc, IServerConfig config, ILogService logService,
            ITextSerializer serializer, ISecretsProvider secretsProvider, ITaskService taskService, ICloudCommsService cloudCommsService, IReflectionService reflectionService, 
            CancellationToken token)
        {
            AzureCloudMessageImpl commsImpl = new AzureCloudMessageImpl();
            await commsImpl.Init(loc, config, logService, serializer, secretsProvider, taskService, cloudCommsService, reflectionService, token);
            return commsImpl;
        }

        public async Task<IRepository> CreateRepo(InitRepoArgs args, CancellationToken token)
        {

            try
            {
                if (_providers.TryGetValue(args.RepoType, out IAzureRepositoryProvider repoProvider))
                {
                    return await repoProvider.TryCreateRepo(args, token);
                }
            }
            catch (Exception ee)
            {
                _logService.Exception(ee, "Azure.CreateRepo");
            }
            return null;
        }

        public string GetPublicIPAddress(IServerConfig config, ILogService logService, ISecretsProvider secretsProvider, CancellationToken token)
        {

            return "";
        }

    }
}



