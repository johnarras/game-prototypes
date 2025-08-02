using Genrpg.ServerShared.CloudComms.Platforms.PubSub;
using Genrpg.ServerShared.CloudComms.PubSub.Constants;
using Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Entities;

namespace Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin
{
    public class AzureAdminPubSubHelper : BaseAzurePubSubHelper<IAdminPubSubMessage, IAdminPubSubMessageHandler>
    {
        public override string BaseTopicName() { return PubSubTopicNames.Admin.ToString(); }
    }
}
