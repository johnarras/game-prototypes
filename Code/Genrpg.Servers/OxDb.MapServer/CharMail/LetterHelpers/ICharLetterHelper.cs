using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.CharMail.PlayerData;
using System.Threading.Tasks;

namespace OxDb.MapServer.CharMail.LetterHelpers
{
    public interface ICharLetterHelper : ISetupDictionaryItem<long>
    {
        Task ProcessLetter(Character ch, CharLetter letter);
    }
}


