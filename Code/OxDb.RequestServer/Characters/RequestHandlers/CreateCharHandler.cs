using OxDb.RequestServer.Core;
using OxDb.RequestServer.GameClientRequests.RequestHandlers;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Characters.WebApi.CreateChar;

namespace OxDb.RequestServer.Characters.RequestHandlers
{
    public class CreateCharHandler : BaseClientUserRequestHandler<CreateCharRequest>
    {
        protected override async Task InnerHandleMessage(WebContext context, CreateCharRequest request, CancellationToken token)
        {


            List<CharacterStub> charStubs = await _playerDataService.LoadCharacterStubs(context.GameUserId);

            int nextId = 1;

            while (true)
            {
                if (charStubs.FirstOrDefault(x => x.Id == context.GameUserId + "." + nextId) == null)
                {
                    break;
                }
                nextId++;
            }

            CoreCharacter coreCh = new CoreCharacter()
            {
                Id = context.GameUserId + "." + nextId,
                Name = request.Name,
                UserId = context.GameUserId,
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

            context.AddResponse(response);

        }
    }
}


