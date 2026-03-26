using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.CharMail.PlayerData;
using Genrpg.Shared.Interfaces;
using System.Threading.Tasks;

namespace Genrpg.MapServer.CharMail.LetterHelpers
{
    public interface ICharLetterHelper : ISetupDictionaryItem<long>
    {
        Task ProcessLetter(Character ch, CharLetter letter);
    }
}


