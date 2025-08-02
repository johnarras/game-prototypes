using Genrpg.RequestServer.BoardGame.Entities;
using Genrpg.RequestServer.Core;
using Genrpg.Shared.BoardGame.Constants;

namespace Genrpg.RequestServer.BoardGame.Modes.Helpers
{
    /// <summary>
    /// Take a lap around another player's board and damage any special tiles you land on.
    /// Their guard posts make you spend mana to defeat, or use gems. Rewards increase
    /// the farther you go along.
    /// </summary>
    public class PVPBoardModeHelper : BaseBoardModeHelper
    {
        public override long Key => BoardModes.PVP;
        public override EBonusModeEndTypes BonusModeEndType => EBonusModeEndTypes.HomeTile;
        public override long TriggerTileTypeId => 0;
        protected override EPlayRollTypes PlayMultTypes => EPlayRollTypes.Current;

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
