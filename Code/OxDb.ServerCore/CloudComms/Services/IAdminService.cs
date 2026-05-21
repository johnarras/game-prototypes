using OxDb.ServerCore.CloudComms.PubSub.Topics.Admin.Messages;
using OxDb.SharedCore.Interfaces;

namespace OxDb.ServerCore.CloudComms.Services
{
    public interface IAdminService : IInjectable
    {
        Task HandleReloadGameState();
        Task OnServerStarted(ServerStartedAdminMessage message);
        Task OnMapUploaded(MapUploadedAdminMessage message);
    }
}


