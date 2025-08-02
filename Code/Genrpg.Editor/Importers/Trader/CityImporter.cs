using Genrpg.Editor.Constants;
using Genrpg.Editor.Entities.Core;
using Genrpg.Shared.Trader.Animals.Settings;
using Genrpg.Shared.Trader.Cities.Settings;
using Genrpg.Shared.Trader.TradeGoods.Settings;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Editor.Importers.Trader
{
    public class CityImportRow
    {
        public long IdKey { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public string AncientName { get; set; }
        public long Population { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int MapPixelX { get; set; }
        public int MapPixelY { get; set; }
        public string PrimaryProducts { get; set; }
        public string CommonlyAvailableAnimals { get; set; }
        public string UncommonlyAvailableAnimals { get; set; }
    }

    public class CityImporter : BaseTraderDataImporter<CitySettings, City>
    {

        private ITextSerializer _textSerializer = null;

        public override string ImportDataFilename => "CitiesImport.csv";

        public override EImportTypes Key => EImportTypes.Cities;

        protected override void ImportChildSubObject(EditorGameState gs, City current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
        }

        protected override async Task<bool> ParseInputFromLines(WindowBase window, EditorGameState gs, List<string[]> lines)
        {
            CitySettings settings = gs.data.Get<CitySettings>(null);

            List<City> newList = new List<City>();

            Dictionary<string, string[]> headers = new Dictionary<string, string[]>();

            IReadOnlyList<Animal> animals = gs.data.Get<AnimalSettings>(null).GetData();
            IReadOnlyList<TradeGood> tradeGoods = gs.data.Get<TradeGoodSettings>(null).GetData();

            StringBuilder sb = new StringBuilder();
            string childTypeName = typeof(City).Name.ToLower();
            City currentChild = null;
            for (int row = 0; row < lines.Count; row++)
            {
                string[] rowWords = lines[row];

                if (rowWords.Length < 2 || string.IsNullOrEmpty(rowWords[0]))
                {
                    continue;
                }

                rowWords[0] = rowWords[0].ToLower();

                if (rowWords[0].IndexOf("header") >= 0)
                {
                    string headerWord = rowWords[0].Replace("header", "").Trim();

                    headers[headerWord] = rowWords;
                    continue;
                }
                if (rowWords[0] == childTypeName)
                {
                    CityImportRow importRow = _importService.ImportLine<CityImportRow>(gs, row, rowWords, headers[childTypeName]);

                    currentChild = _textSerializer.ConvertType<CityImportRow, City>(importRow);

                    newList.Add(currentChild);

                    string[] productNames = importRow.PrimaryProducts.Split(';');

                    foreach (string productName in productNames)
                    {

                        TradeGood tg = tradeGoods.FirstOrDefault(x => StrUtils.NormalizeWord(productName) == StrUtils.NormalizeWord(x.Name));
                        if (tg == null)
                        {
                            sb.Append("Bad product name " + productName + " in row " + row + "\n");
                            continue;
                        }
                        currentChild.TradeGoods.Add(new CityTradeGood() { TradeGoodId = tg.IdKey });
                    }

                    List<string[]> animalNameLists = new List<string[]>();

                    animalNameLists.Add(importRow.CommonlyAvailableAnimals.Split(";"));
                    animalNameLists.Add(importRow.UncommonlyAvailableAnimals.Split(";"));

                    for (int i = 0; i < animalNameLists.Count; i++)
                    {
                        foreach (string animalName in animalNameLists[i])
                        {
                            Animal animal = animals.FirstOrDefault(x => StrUtils.NormalizeWord(animalName) == StrUtils.NormalizeWord(x.Name));

                            if (animal == null)
                            {
                                sb.Append("Bad animal name: " + animalName + " in row " + row + "\n");
                                continue;
                            }

                            currentChild.Animals.Add(new CityAnimal() { AnimalId = animal.IdKey, PriceScale = (i == 0 ? 1 : 10) });
                        }
                    }
                }
            }

            String txt = sb.ToString();
            if (!string.IsNullOrEmpty(txt))
            {
                throw new Exception(txt);
            }

            settings.SetData(newList);
            gs.LookedAtObjects.AddRange(newList);

            await Task.CompletedTask;
            return true;
        }
    }
}
