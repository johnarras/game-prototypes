using OxDb.DataUtils.Entities.Core;
using OxDb.DataUtils.Importers.Core;
using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedGame.Attributes.Constants;
using OxDb.SharedGame.Attributes.Settings;

namespace OxDb.DataUtils.Importers.Gameplay
{
    public class GameplayBuffImporter : ParentChildImporter<GameplayBuffSettings, GameplayBuff>
    {
        protected override void ImportChildSubObject(EditorGameState gs, GameplayBuff current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
            if (firstColumn == "buffeffect")
            {
                Effect effect = _importService.ImportLine<Effect>(gs, row, headers, rowWords);

                if (!_attributeService.EntityTypeHasValIndex(effect.EntityTypeId, EAttributeValIndex.Buff))
                {
                    throw new Exception($"Buff Importer row{row} has non-buff entity type in it's effect.");
                }

                current.Effects.Add(effect);
            }
        }
    }
}
