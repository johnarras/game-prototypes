using Genrpg.DataUtils.Entities.Core;
using Genrpg.Shared.Crawler.Spells.Settings;

namespace Genrpg.DataUtils.Importers.Crawler
{

    public class CrawlerSpellSettingsImporter : BaseCrawlerDataImporter<CrawlerSpellSettings>
    {
        protected override async Task<bool> ParseInputFromLines(EditorGameState gs, List<string[]> lines)
        {
            string[] spellHeaderLine = lines[0];
            string[] effectHeaders = lines[1];

            CrawlerSpellSettings spellSettings = gs.data.Get<CrawlerSpellSettings>(null);
            List<CrawlerSpell> crawlerSpells = new List<CrawlerSpell>();

            List<CrawlerSpell> oldSpells = spellSettings.GetData().ToList();

            CrawlerSpell currentSpell = null;
            for (int l = 2; l < lines.Count; l++)
            {
                string[] rowWords = lines[l];

                if (rowWords[0] == "spell")
                {
                    if (rowWords.Length < 3)
                    {
                        continue;
                    }

                    currentSpell = _importService.ImportLine<CrawlerSpell>(gs, l, spellHeaderLine, rowWords);

                    crawlerSpells.Add(currentSpell);

                    currentSpell.Effects = new List<CrawlerSpellEffect>();
                }
                else if (rowWords[0] == "effect")
                {
                    CrawlerSpellEffect effect = _importService.ImportLine<CrawlerSpellEffect>(gs, l, effectHeaders, rowWords);
                    currentSpell.Effects.Add(effect);
                }
                // Otherwise could be blank or informational row.
            }
            foreach (CrawlerSpell crawlerSpell in crawlerSpells)
            {
                gs.LookedAtObjects.Add(crawlerSpell);
            }
            spellSettings.SetData(crawlerSpells);

            await Task.CompletedTask;
            return true;
        }
    }
}

