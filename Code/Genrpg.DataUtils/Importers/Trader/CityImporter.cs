using Genrpg.DataUtils.Entities.Core;
using Genrpg.Shared.Serialization.Interfaces;
using Genrpg.Shared.Trader.Animals.Settings;
using Genrpg.Shared.Trader.Biomes.Settings;
using Genrpg.Shared.Trader.Cities.Settings;
using Genrpg.Shared.Trader.Cultures.Settings;
using Genrpg.Shared.Trader.TradeGoods.Settings;
using Genrpg.Shared.Utils;
using System.Text;

namespace Genrpg.DataUtils.Importers.Trader
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
        public string BiomeName { get; set; }
        public string CultureName { get; set; }
        public string PrimaryProducts { get; set; }
        public string CommonlyAvailableAnimals { get; set; }
        public string UncommonlyAvailableAnimals { get; set; }
    }

    public class CityImporter : BaseTraderDataImporter<CitySettings, City>
    {
        private ITextSerializer _textSerializer = null;

        protected override void ImportChildSubObject(EditorGameState gs, City current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
        }

        protected override async Task<bool> ParseInputFromLines(EditorGameState gs, List<string[]> lines)
        {
            CitySettings settings = gs.data.Get<CitySettings>(null);

            List<City> newList = new List<City>();

            Dictionary<string, string[]> headers = new Dictionary<string, string[]>();

            IReadOnlyList<AnimalType> animals = gs.data.Get<AnimalTypeSettings>(null).GetData();
            IReadOnlyList<TradeGood> tradeGoods = gs.data.Get<TradeGoodSettings>(null).GetData();

            BiomeTypeSettings biomeSettings = gs.data.Get<BiomeTypeSettings>(null);
            CultureTypeSettings cultureSettings = gs.data.Get<CultureTypeSettings>(null);

            List<BiomeType> biomeTypes = biomeSettings.GetData().ToList();
            List<CultureType> cultureTypes = cultureSettings.GetData().ToList();


            List<string> badTradeGoods = new List<string>();
            List<string> badAnimals = new List<string>();
            List<string> allTradeGoods = new List<string>();
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
                    CityImportRow importRow = _importService.ImportLine<CityImportRow>(gs, row, headers[childTypeName], rowWords);

                    currentChild = _textSerializer.ConvertType<CityImportRow, City>(importRow);

                    currentChild.BiomeTypeId = _importService.GetOrAddMissingEntity<BiomeTypeSettings, BiomeType>(gs, importRow.BiomeName);
                    currentChild.CultureTypeId = _importService.GetOrAddMissingEntity<CultureTypeSettings, CultureType>(gs, importRow.CultureName);

                    newList.Add(currentChild);

                    List<string> productNames = StrUtils.CommaSemiColonSplit(importRow.PrimaryProducts);

                    foreach (string productName in productNames)
                    {

                        TradeGood tg = tradeGoods.FirstOrDefault(x => StrUtils.NormalizeWord(productName) == StrUtils.NormalizeWord(x.Name));

                        if (!allTradeGoods.Contains(productName))
                        {
                            allTradeGoods.Add(productName);
                        }

                        if (tg == null)
                        {
                            if (!badTradeGoods.Contains(productName))
                            {
                                badTradeGoods.Add(productName);
                            }
                            continue;
                        }
                        currentChild.TradeGoodsProduced.Add(new CityTradeGood() { TradeGoodId = tg.IdKey });
                    }

                    List<List<string>> animalNameLists = new List<List<string>>();
                    animalNameLists.Add(StrUtils.CommaSemiColonSplit(importRow.CommonlyAvailableAnimals));
                    animalNameLists.Add(StrUtils.CommaSemiColonSplit(importRow.UncommonlyAvailableAnimals));

                    for (int i = 0; i < animalNameLists.Count; i++)
                    {
                        foreach (string animalName in animalNameLists[i])
                        {
                            AnimalType animal = animals.FirstOrDefault(x => StrUtils.NormalizeWord(animalName) == StrUtils.NormalizeWord(x.Name));

                            if (animal == null)
                            {
                                if (!badAnimals.Contains(animalName))
                                {
                                    badAnimals.Add(animalName);
                                }
                                continue;
                            }

                            currentChild.Animals.Add(new CityAnimal() { AnimalTypeId = animal.IdKey, PriceScale = (i == 0 ? 1 : 10) });
                        }
                    }
                }
            }

            StringBuilder allTradeSB = new StringBuilder();

            foreach (string tradeSB in allTradeGoods)
            {
                allTradeSB.Append(tradeSB + ";");
            }


            string allTrade = allTradeSB.ToString();
            _logService.Info(allTrade);
            StringBuilder finalErrors = new StringBuilder();

            if (badTradeGoods.Count > 0)
            {
                finalErrors.Append("BadTradeGoods: ");
                foreach (string error in badTradeGoods)
                {
                    finalErrors.Append(error + ";");
                }
                finalErrors.Append("\n");
            }
            if (badAnimals.Count > 0)
            {
                finalErrors.Append("BadAnimals: ");
                foreach (string error in badAnimals)
                {
                    finalErrors.Append(error + ";");
                }
                finalErrors.Append("\n");
            }
            String txt = finalErrors.ToString();
            if (!string.IsNullOrEmpty(txt))
            {
                throw new Exception(txt);
            }

            settings.SetData(newList);
            gs.LookedAtObjects.AddRange(newList);
            gs.LookedAtObjects.Add(settings);

            await Task.CompletedTask;
            return true;
        }
    }
}


