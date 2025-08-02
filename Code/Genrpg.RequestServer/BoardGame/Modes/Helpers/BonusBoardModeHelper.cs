using Genrpg.RequestServer.BoardGame.Entities;
using Genrpg.RequestServer.Core;
using Genrpg.Shared.BoardGame.Constants;

namespace Genrpg.RequestServer.BoardGame.Modes.Helpers
{
    /// <summary>
    /// Take several rolls with different rewards on your own board.
    /// </summary>
    public class BonusBoardModeHelper : BaseBoardModeHelper
    {
        public override long Key => BoardModes.Bonus;
        public override EBonusModeEndTypes BonusModeEndType => EBonusModeEndTypes.RollCount;
        public override long TriggerTileTypeId => 0;
        protected override EPlayRollTypes PlayMultTypes => EPlayRollTypes.Average;

        public override async Task EnterMode(WebContext context, RollDiceArgs rules)
        {
            await Task.CompletedTask;
        }

        public override async Task ExitMode(WebContext context, RollDiceArgs rules)
        {
            await Task.CompletedTask;
        }

    }
}
