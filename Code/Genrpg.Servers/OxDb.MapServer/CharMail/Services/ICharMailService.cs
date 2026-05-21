using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Characters.PlayerData;
using System.Threading.Tasks;

namespace OxDb.MapServer.CharMail.Services
{
    public interface ICharMailService : IInjectable
    {
        Task ProcessMail(Character ch, string charLetterID);
    }
}


