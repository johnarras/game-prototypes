using Genrpg.RequestServer.ClientUserRequests.RequestHandlers;
using Genrpg.RequestServer.Core;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Characters.WebApi.DeleteChar;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;

namespace Genrpg.RequestServer.Characters.RequestHandlers
{
    public class DeleteCharHandler : BaseClientUserRequestHandler<DeleteCharRequest>
    {
        protected override async Task InnerHandleMessage(WebContext context, DeleteCharRequest request, CancellationToken token)
        {
            CoreCharacter coreCh = await _repoService.Load<CoreCharacter>(request.CharId);

            if (coreCh != null && coreCh.UserId == context.core.Id)
            {
                Character ch = new Character(coreCh);

                await _playerDataService.LoadAllPlayerData(context.rand, context.GameUserId, context.AllData(), ch);
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



