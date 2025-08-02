
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Characters.Utils;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Entities.Constants;
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.ClientUserRequests.RequestHandlers;
using Genrpg.Shared.Characters.WebApi.CreateChar;

namespace Genrpg.RequestServer.Characters.RequestHandlers
{
    public class CreateCharHandler : BaseClientUserRequestHandler<CreateCharRequest>
    {
        protected override async Task InnerHandleMessage(WebContext context, CreateCharRequest request, CancellationToken token)
        {
            List<CharacterStub> charStubs = await _playerDataService.LoadCharacterStubs(context.user.Id);

            int nextId = 1;

            while (true)
            {
                if (charStubs.FirstOrDefault(x => x.Id == context.user.Id + "." + nextId) == null)
                {
                    break;
                }
                nextId++;
            }

            CoreCharacter coreCh = new CoreCharacter()
            {
                Id = context.user.Id + "." + nextId,
                Name = request.Name,
                UserId = context.user.Id,
                Level = 1,
                EntityTypeId = EntityTypes.Unit,
                EntityId = request.UnitTypeId,
                SexTypeId = request.SexTypeId,
            };
            Character ch = new Character(coreCh);
            await _repoService.Save(coreCh);

            charStubs.Add(new CharacterStub() { Id = coreCh.Id, Name = coreCh.Name, Level = coreCh.Level });

            CreateCharResponse response = new CreateCharResponse()
            {
                NewChar = _serializer.ConvertType<Character, Character>(ch),
                AllCharacters = charStubs,
            };

            context.Responses.AddResponse(response);

        }
    }
}
