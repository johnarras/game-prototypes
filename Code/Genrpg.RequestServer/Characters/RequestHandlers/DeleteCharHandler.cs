using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Characters.Utils;
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.ClientUserRequests.RequestHandlers;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.Characters.WebApi.DeleteChar;

namespace Genrpg.RequestServer.Characters.RequestHandlers
{
    public class DeleteCharHandler : BaseClientUserRequestHandler<DeleteCharRequest>
    {
        protected override async Task InnerHandleMessage(WebContext context, DeleteCharRequest request, CancellationToken token)
        {
            CoreCharacter coreCh = await _repoService.Load<CoreCharacter>(request.CharId);

            if (coreCh != null && coreCh.UserId == context.user.Id)
            {
                Character ch = new Character(coreCh);

                await _playerDataService.LoadAllPlayerData(context.rand, context.user, ch);
                await _repoService.Delete(coreCh);

                foreach (IUnitData data in ch.GetAllData().Values)
                {
                    if (data.Id != context.user.Id) // Do not delete user data
                    {
                        _repoService.QueueDelete(data);
                    }
                }
                coreCh = null;
            }

            DeleteCharResponse response = new DeleteCharResponse()
            {
                AllCharacters = await _playerDataService.LoadCharacterStubs(context.user.Id),
            };

            context.Responses.AddResponse(response);
        }
    }
}

