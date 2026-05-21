using OxDb.ServerCore.CloudComms.PubSub.Entities;
using OxDb.ServerCore.CloudComms.Services;
using OxDb.SharedCore.Logalytics.Interfaces;

namespace OxDb.ServerCore.CloudComms.PubSub.Topics.Admin.Entities
{
    public abstract class BaseAdminPubSubMessageHandler<M> : BasePubSubMessageHandler<M>, IAdminPubSubMessageHandler where M : class, IPubSubMessage
    {
        protected IAdminService _adminService = null;
        protected ILogService _logService = null;
    }
}


