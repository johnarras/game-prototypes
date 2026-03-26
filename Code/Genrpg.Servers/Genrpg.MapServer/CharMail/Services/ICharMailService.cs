using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Interfaces;
using System.Threading.Tasks;

namespace Genrpg.MapServer.CharMail.Services
{
    public interface ICharMailService : IInjectable
    {
        Task ProcessMail(Character ch, string charLetterID);
    }
}


