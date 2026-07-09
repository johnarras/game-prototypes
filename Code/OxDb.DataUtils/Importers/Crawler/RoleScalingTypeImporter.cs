using OxDb.DataUtils.Entities.Core;
using OxDb.DataUtils.Importers.Core;
using OxDb.SharedGame.Crawler.Roles.Settings;

namespace OxDb.DataUtils.Importers.Crawler
{

    public class RoleScalingTypeSettingsImporter : ParentChildImporter<RoleScalingTypeSettings, RoleScalingType>
    {
        protected override void ImportSubobject(EditorGameState gs, RoleScalingTypeSettings settings, RoleScalingType current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
        }
    }
}


