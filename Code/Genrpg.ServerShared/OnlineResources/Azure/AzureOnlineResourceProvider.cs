using Azure.Core;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager;
using Genrpg.ServerShared.CloudComms.Platforms;
using Genrpg.ServerShared.CloudComms.Services;
using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.DataStores.Blobs;
using Genrpg.ServerShared.DataStores.Entities;
using Genrpg.ServerShared.DataStores.NoSQL;
using Genrpg.ServerShared.OnlineResources.Interfaces;
using Genrpg.ServerShared.Secrets.Services;
using Genrpg.Shared.Analytics.Services;
using Genrpg.Shared.DataStores.DataGroups;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Tasks.Services;
using Genrpg.Shared.Utils;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.OnlineResources.Azure
{

    public class AzureOnlineResourceProvider : IOnlineResourceProvider
    {
        private ILogService _logService = null;
        private IAnalyticsService _analyticsService = null;
        private ITextSerializer _textSerializer = null;
        private ISecretsProvider _secretsProvider = null;

        public async Task<ICloudMessageImpl> CreateCloudMessageImpl(IServiceLocator loc, IServerConfig config, ILogService logService,
            ITextSerializer serializer, ISecretsProvider secretsProvider, ITaskService taskService, ICloudCommsService cloudCommsService, CancellationToken token)
        {
            AzureCloudMessageImpl commsImpl = new AzureCloudMessageImpl();
            await commsImpl.Init(loc, config, logService, serializer, secretsProvider, taskService, cloudCommsService, token);
            return commsImpl;
        }

        public async Task<IRepository> CreateRepo(InitRepoArgs args)
        {
            string repoStr = args.RepoType.ToString();
            string categoryStr = args.Category.ToString();

            string secretId = repoStr + categoryStr;
            string connectionString = await _secretsProvider.GetSecret(repoStr+categoryStr);

            if (args.RepoType == ERepoTypes.NoSQL)
            {
                AzureCosmosMongoRepository repo = new AzureCosmosMongoRepository();
                await repo.Init(args, connectionString, _logService, _analyticsService, _textSerializer);
                return repo;
            }
            else if (args.RepoType == ERepoTypes.Blob)
            {
                AzureBlobRepository repo = new AzureBlobRepository();
                await repo.Init(args, connectionString, _logService, _analyticsService, _textSerializer);
                return repo;
            }

            return null;
        }

        public string GetPublicIPAddress(IServerConfig config, ILogService logService, ISecretsProvider secretsProvider, CancellationToken token)
        {
            //var armClient = new ArmClient(new DefaultAzureCredential());

            //var subscription = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscriptionId));
            //var resourceGroup = subscription.GetResourceGroup(resourceGroupName);
            //var vm = await resourceGroup.GetVirtualMachines().GetAsync(vmName);

            //var nicRef = vm.Value.Data.NetworkProfile.NetworkInterfaces.FirstOrDefault();
            //if (nicRef == null) return null;

            //var nic = armClient.GetNetworkInterfaceResource(new ResourceIdentifier(nicRef.Id));
            //var ipConfig = await nic.GetNetworkInterfaceIPConfigurations().FirstOrDefault()?.GetAsync();
            //if (ipConfig == null || ipConfig.Value.Data.PublicIPAddress == null) return null;

            //var publicIp = armClient.GetPublicIPAddressResource(new ResourceIdentifier(ipConfig.Value.Data.PublicIPAddress.Id));
            //var publicIpData = await publicIp.GetAsync();

            //return publicIpData.Value.Data.IPAddress;

            return "";
        }

    }
}

