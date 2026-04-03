using Genrpg.DataUtils.Entities.Core;
using Genrpg.DataUtils.Importers.Core;
using Genrpg.Shared.Attributes.Constants;
using Genrpg.Shared.Attributes.Settings;
using Genrpg.Shared.Effects.Entities;

namespace Genrpg.DataUtils.Importers.Gameplay
{
    public class GameplayDebuffImporter : ParentChildImporter<GameplayDebuffSettings, GameplayDebuff>
    {
        protected override void ImportChildSubObject(EditorGameState gs, GameplayDebuff current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
            if (firstColumn == "debuffeffect")
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
