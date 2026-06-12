using OxDb.DataUtils.Entities.Core;
using OxDb.DataUtils.Importers.Core;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Riddles.Settings;
using System.Text;

namespace OxDb.DataUtils.Importers.Riddles
{

    public class RiddleSettingsImporter : BaseParentDataImporter<RiddleSettings>
    {

        protected override async Task<bool> ParseInputFromLines(EditorGameState gs, List<string[]> lines)
        {
            RiddleSettings settings = gs.data.Get<RiddleSettings>(null);

            List<Riddle> riddles = new List<Riddle>();

            long riddleId = 0;


            for (int i = 0; i < lines.Count; i++)
            {
                if (i >= lines.Count)
                {
                    break;
                }

                string[] words = lines[i];

                if (StrUtils.IsEmptyLine(words))
                {
                    continue;
                }

                Riddle riddle = new Riddle()
                {
                    IdKey = ++riddleId
                };

                StringBuilder desc = new StringBuilder();
                while (i < lines.Count && StrUtils.IsEmptyLine(lines[i]))
                {
                    i++;
                }

                if (i >= lines.Count)
                {
                    break;
                }

                while (i < lines.Count && !StrUtils.IsEmptyLine(lines[i]))
                {
                    desc.Append(StrUtils.RecombineCSVLine(lines[i]) + "\n");
                    i++;
                }

                riddle.Desc = desc.ToString();

                if (i >= lines.Count)
                {
                    break;
                }

                while (i < lines.Count && StrUtils.IsEmptyLine(lines[i]))
                {
                    i++;
                }

                if (i >= lines.Count)
                {
                    break;
                }

                riddle.Name = StrUtils.RecombineCSVLine(lines[i]);

                riddles.Add(riddle);
                gs.LookedAtObjects.Add(riddle);
            }

            settings.SetData(riddles);

            gs.LookedAtObjects.Add(settings);

            await Task.CompletedTask;
            return true;
        }
    }
}


