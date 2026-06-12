using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Names.Constants;
using OxDb.SharedCore.Utils;
using System.Linq;
using System.Threading.Tasks;

namespace OxDb.SharedCore.Names.Services
{
    public class ValidateNameResult
    {
        public bool Ok { get; set; }
        public string ErrorMessage { get; set; }
    }

    public interface INameValidationService : IInjectable
    {
        Task<ValidateNameResult> ValidateName(string DisplayName);

        Task<bool> ContainsSwearWord(string word);
    }

    public class NameValidationService : INameValidationService
    {



        // This needs to be improved obviously.
        private readonly string[] _nameBlacklist = {
            "fuck", "shit", "nazi", "cunt",
            "piss", "slut", "nigg", "damn",
            "hell", "asshole", "fuk", "shyt",
            "coc", "dik", "vag",

        };


        public async Task<bool> ContainsSwearWord(string word)
        {
            string lowerId = word.ToLower();

            // Check for direct matches or leetspeak subs
            // You can expand this to check for '5' as 's', etc.
            string normalized = lowerId
                .Replace('5', 's')
                .Replace('1', 'i')
                .Replace('4', 'a')
                .Replace('8', 'b')
                .Replace('0', 'o')
                .Replace('3', 'e')
                .Replace('6', 'g')
                ;

            await Task.CompletedTask;
            return _nameBlacklist.Any(word => normalized.Contains(word));
        }

        public async Task<ValidateNameResult> ValidateName(string DisplayName)
        {
            ValidateNameResult result = new ValidateNameResult();

            if (await ContainsSwearWord(DisplayName))
            {
                result.ErrorMessage = "Cannot use swear words.";
                return result;
            }

            string alNumString = StrUtils.GetAlNumSubstring(DisplayName);

            string DisplayNameError = $"Your Visible Name must be between {NameConstants.MinDisplayNameLength} and {NameConstants.MaxDisplayNameLength} alphanumeric characters.";
            if (alNumString != DisplayName ||
                string.IsNullOrEmpty(alNumString) ||
                alNumString.Length < NameConstants.MinDisplayNameLength ||
                alNumString.Length > NameConstants.MaxDisplayNameLength)
            {

                result.ErrorMessage = DisplayNameError;
                return result;
            }

            result.Ok = true;
            return result;
        }
    }
}
