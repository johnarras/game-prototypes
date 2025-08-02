using Genrpg.Editor.Constants;
using Genrpg.Editor.Entities.Core;
using Genrpg.Editor.Importers.Core;
using Genrpg.Shared.UserCoins.Settings;

namespace Genrpg.Editor.Importers
{
    public class UserCoinTypeImporter : ParentChildImporter<UserCoinSettings, UserCoinType>
    {
        public override string ImportDataFilename => "UserCoinImport.csv";

        public override EImportTypes Key => EImportTypes.UserCoins;

        protected override void ImportChildSubObject(EditorGameState gs, UserCoinType current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
        }
    }
}
