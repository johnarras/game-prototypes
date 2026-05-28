using OxDb.ServerCore.AzureImpl.DataStores.Entities;
using OxDb.ServerCore.CloudComms.Services;
using OxDb.ServerCore.Config;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedCore.Tasks.Services;
using OxDb.SharedCore.Utils;

namespace OxDb.ServerCore.DataStores.Services
{
    public interface IOnlineResourceProvider : IInjectable
    {
        public Task<IRepository> CreateRepo(InitRepoArgs args, CancellationToken token);
        public Task<ICloudMessageImpl> CreateCloudMessageImpl(IServiceLocator loc,
            IServerConfig config, ILogService logService, ITextSerializer serializer,
            ITaskService taskService,
            ICloudCommsService cloudCommsService, IReflectionService reflectionService, CancellationToken token);

        public string GetPublicIPAddress(IServerConfig config, ILogService logService, CancellationToken token);
    }
}


