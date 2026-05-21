using OxDb.ServerCore.CloudComms.PubSub.Constants;
using OxDb.ServerCore.CloudComms.PubSub.Topics.Admin.Entities;

namespace OxDb.ServerCore.AzureImpl.CloudComms.Platforms.PubSub
{
    public class AzureAdminPubSubHelper : BaseAzurePubSubHelper<IAdminPubSubMessage, IAdminPubSubMessageHandler>
    {
        public override string BaseTopicName() { return PubSubTopicNames.Admin.ToString(); }
    }
}


