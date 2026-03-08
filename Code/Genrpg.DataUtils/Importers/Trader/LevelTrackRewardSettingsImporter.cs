using Genrpg.DataUtils.Entities.Core;
using Genrpg.DataUtils.Importers.Core;
using Genrpg.Shared.LevelTracks.Settings;

namespace Genrpg.DataUtils.Importers.Trader
{
    public class LevelTrackRewardSettingsImporter : ParentChildImporter<LevelTrackRewardSettings, LevelTrackReward>
    {
        protected override void ImportChildSubObject(EditorGameState gs, LevelTrackReward current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
        }
    }
}


