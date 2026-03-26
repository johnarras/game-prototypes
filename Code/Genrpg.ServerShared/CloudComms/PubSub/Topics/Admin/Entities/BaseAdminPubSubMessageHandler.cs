using Genrpg.ServerShared.CloudComms.PubSub.Entities;
using Genrpg.ServerShared.CloudComms.Services.Admin;
using Genrpg.Shared.Logging.Interfaces;

namespace Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Entities
{
    public abstract class BaseAdminPubSubMessageHandler<M> : BasePubSubMessageHandler<M>, IAdminPubSubMessageHandler where M : class, IPubSubMessage
    {
        protected IAdminService _adminService = null;
        protected ILogService _logService = null;
    }
}


