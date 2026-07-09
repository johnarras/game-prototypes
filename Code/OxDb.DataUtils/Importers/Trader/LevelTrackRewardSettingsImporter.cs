using OxDb.DataUtils.Entities.Core;
using OxDb.DataUtils.Importers.Core;
using OxDb.SharedGame.LevelTracks.Settings;

namespace OxDb.DataUtils.Importers.Trader
{
    public class LevelTrackRewardSettingsImporter : ParentChildImporter<LevelTrackRewardSettings, LevelTrackReward>
    {
        protected override void ImportSubobject(EditorGameState gs, LevelTrackRewardSettings settings, LevelTrackReward current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
        }
    }
}


