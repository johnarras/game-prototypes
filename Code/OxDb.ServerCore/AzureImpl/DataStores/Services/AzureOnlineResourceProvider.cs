using OxDb.ServerCore.AzureImpl.CloudComms.Platforms;
using OxDb.ServerCore.AzureImpl.DataStores.Entities;
using OxDb.ServerCore.CloudComms.Services;
using OxDb.ServerCore.Config;
using OxDb.ServerCore.DataStores.Services;
using OxDb.SharedCore.DataStores.DataGroups;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.HelperClasses;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedCore.Tasks.Services;
using OxDb.SharedCore.Utils;

namespace OxDb.ServerCore.AzureImpl.DataStores.Services
{

    public class AzureOnlineResourceProvider : IOnlineResourceProvider
    {
        private ILogService _logService = null;

        private SetupDictionaryContainer<ERepoTypes, IAzureRepositoryProvider> _providers = new SetupDictionaryContainer<ERepoTypes, IAzureRepositoryProvider>();

        public async Task<ICloudMessageImpl> CreateCloudMessageImpl(IServiceLocator loc, IServerConfig config, ILogService logService,
            ITextSerializer serializer, ITaskService taskService, ICloudCommsService cloudCommsService, IReflectionService reflectionService,
            CancellationToken token)
        {
            AzureCloudMessageImpl commsImpl = new AzureCloudMessageImpl();
            await commsImpl.Init(loc, config, logService, serializer, taskService, cloudCommsService, reflectionService, token);
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

        public string GetPublicIPAddress(IServerConfig config, ILogService logService, CancellationToken token)
        {

            return "";
        }

    }
}



