using Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Messages;
using Genrpg.Shared.Interfaces;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudComms.Services.Admin
{
    public interface IAdminService : IInjectable
    {
        Task HandleReloadGameState();
        Task OnServerStarted(ServerStartedAdminMessage message);
        Task OnMapUploaded(MapUploadedAdminMessage message);
    }
}


