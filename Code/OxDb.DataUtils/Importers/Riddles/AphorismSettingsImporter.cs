using OxDb.DataUtils.Entities.Core;
using OxDb.DataUtils.Importers.Core;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Riddles.Settings;

namespace OxDb.DataUtils.Importers.Riddles
{
    public class AphorismSettingsImporter : BaseParentDataImporter<AphorismSettings>
    {

        protected override async Task<bool> ParseInputFromLines(EditorGameState gs, List<string[]> lines)
        {
            AphorismSettings settings = gs.data.Get<AphorismSettings>(null);


            List<Aphorism> newList = new List<Aphorism>();

            for (int line = 0; line < lines.Count; line++)
            {

                Aphorism aph = new Aphorism()
                {
                    IdKey = line + 1,
                    Desc = StrUtils.RecombineCSVLine(lines[line]),
                    Name = "Aph" + (line + 1),
                };
                newList.Add(aph);
                gs.LookedAtObjects.Add(aph);
            }

            settings.SetData(newList);

            gs.LookedAtObjects.Add(settings);
            await Task.CompletedTask;
            return true;
        }
    }
}


