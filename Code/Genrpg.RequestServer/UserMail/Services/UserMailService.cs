using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.UserMail.LetterHelpers;
using Genrpg.ServerShared.DataStores;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.UserMail.PlayerData;

namespace Genrpg.RequestServer.UserMail.Services
{
    public class UserMailService : IUserMailService
    {
        SetupDictionaryContainer<long, IUserLetterHelper> _mailHelpers = new SetupDictionaryContainer<long, IUserLetterHelper>();

        protected IFullRepositoryService _repoService;

        public async Task ProcessMail(WebContext context)
        {
            List<UserLetter> letters = await _repoService.Search<UserLetter>(x => x.OwnerId == context.GameUserId);

            List<Task> deleteTasks = new List<Task>();
            foreach (UserLetter letter in letters)
            {
                if (_mailHelpers.TryGetValue(letter.UserMailTypeId, out IUserLetterHelper userMailHelper))
                {
                    await userMailHelper.ProcessLetter(context, letter);
                }

                deleteTasks.Add(_repoService.Delete(letter));
            }

            await Task.WhenAll(deleteTasks);

        }
    }
}


