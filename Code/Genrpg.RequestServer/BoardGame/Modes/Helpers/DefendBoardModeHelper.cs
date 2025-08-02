using Genrpg.RequestServer.BoardGame.Entities;
using Genrpg.RequestServer.Core;
using Genrpg.Shared.BoardGame.Constants;

namespace Genrpg.RequestServer.BoardGame.Modes.Helpers
{
    /// <summary>
    /// You take a lap around your board with lots of monsters to fight, your guard posts kill some, rewards
    /// increase as you go.
    /// When you land on monsters, spend mana to defeat them, or you spend gems. Prizes increase. Any special
    /// building past where you stopped get destroyed.
    /// </summary>
    public class DefendBoardModeHelper : BaseBoardModeHelper
    {

        public override long Key => BoardModes.Defend;
        public override EBonusModeEndTypes BonusModeEndType => EBonusModeEndTypes.HomeTile;
        public override long TriggerTileTypeId => 0;
        protected override EPlayRollTypes PlayMultTypes => EPlayRollTypes.Average;
        public override bool UseOwnerBoardWhenSwitching() { return true; }

        public override async Task EnterMode(WebContext context, RollDiceArgs args)
        {
            await Task.CompletedTask;
        }

        public override async Task ExitMode(WebContext context, RollDiceArgs args)
        {
            await Task.CompletedTask;
        }
    }
}
