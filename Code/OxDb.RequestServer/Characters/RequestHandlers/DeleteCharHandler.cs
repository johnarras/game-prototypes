using OxDb.RequestServer.ClientUserRequests.RequestHandlers;
using OxDb.RequestServer.Core;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Characters.WebApi.DeleteChar;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;

namespace OxDb.RequestServer.Characters.RequestHandlers
{
    public class DeleteCharHandler : BaseClientUserRequestHandler<DeleteCharRequest>
    {
        protected override async Task InnerHandleMessage(WebContext context, DeleteCharRequest request, CancellationToken token)
        {
            CoreCharacter coreCh = await _repoService.Load<CoreCharacter>(request.CharId);
            CoreData coreData = await context.GetAsync<CoreData>();
            if (coreCh != null && coreCh.UserId == coreData.Id)
            {
                Character ch = new Character(coreCh);

                await _playerDataService.LoadAllPlayerData(context.Rand, context.GameUserId, context.AllData(), ch);
                await _repoService.Delete(coreCh);

                foreach (IUnitData data in ch.GetAllData())
                {
                    if (data.Id != context.GameUserId) // Do not delete user data
                    {
                        await _repoService.Delete(data);
                    }
                }
                coreCh = null;
            }

            DeleteCharResponse response = new DeleteCharResponse()
            {
                AllCharacters = await _playerDataService.LoadCharacterStubs(context.GameUserId),
            };

            context.AddResponse(response);
        }
    }
}



