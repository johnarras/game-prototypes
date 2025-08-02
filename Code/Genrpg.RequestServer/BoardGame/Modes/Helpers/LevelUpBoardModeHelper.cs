using Genrpg.RequestServer.BoardGame.Entities;
using Genrpg.RequestServer.Core;
using Genrpg.Shared.BoardGame.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.BoardGame.Modes.Helpers
{
    /// <summary>
    /// Take a lap around your board once you level up.
    /// Lots of tokens + energy here
    /// </summary>
    public class LevelUpBoardModeHelper : BaseBoardModeHelper
    {
        public override long Key => BoardModes.LevelUp;
        public override EBonusModeEndTypes BonusModeEndType => EBonusModeEndTypes.StartTile;
        public override long TriggerTileTypeId => 0;
        protected override EPlayRollTypes PlayMultTypes => EPlayRollTypes.Average;

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
