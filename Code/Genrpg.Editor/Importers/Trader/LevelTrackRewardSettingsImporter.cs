using Genrpg.Editor.Entities.Core;
using Genrpg.Editor.Importers.Core;
using Genrpg.Shared.LevelTracks.Settings;

namespace Genrpg.Editor.Importers.Trader
{
    public class LevelTrackRewardSettingsImporter : ParentChildImporter<LevelTrackRewardSettings, LevelTrackReward>
    {
        protected override void ImportChildSubObject(EditorGameState gs, LevelTrackReward current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
        }
    }
}


