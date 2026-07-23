using OxDb.RequestServer.Core;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedGame.AI.Settings;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Factions.Constants;
using OxDb.SharedGame.Units.Constants;

namespace OxDb.RequestServer.PlayerData.LoadUpdateHelpers
{
    public class CoreCharacterLoadUpdater : BaseCharacterLoadUpdater
    {
        private IGameData _gameData = null;
        public override ECharacterLoadUpdateOrder HelperKey => ECharacterLoadUpdateOrder.Core;


        public override async Task Update(WebContext context, Character ch)
        {
            ch.FactionTypeId = FactionTypes.Player;
            ch.BaseSpeed = _gameData.Get<AISettings>(ch).BaseUnitSpeed;
            ch.Speed = ch.BaseSpeed;
            ch.RemoveFlag(UnitFlags.Evading);
            ch.EntityTypeId = EntityTypes.Unit;
            ch.EntityId = 1;
            await Task.CompletedTask;
        }
    }
}


