using Genrpg.Editor.Constants;
using Genrpg.Editor.Entities.Core;
using Genrpg.Editor.Importers.Core;
using Genrpg.Shared.LevelTracks.Settings;

namespace Genrpg.Editor.Importers.Trader
{
    public class LevelTrackRewardsImporter : ParentChildImporter<LevelTrackRewardSettings, LevelTrackReward>
    {
        public override string ImportDataFilename => "LevelTrackRewardsImport.csv";

        public override EImportTypes HelperKey => EImportTypes.LevelTrackRewards;

        protected override void ImportChildSubObject(EditorGameState gs, LevelTrackReward current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
        }
    }
}
