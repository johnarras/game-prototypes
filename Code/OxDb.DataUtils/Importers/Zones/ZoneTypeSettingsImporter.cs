using OxDb.DataUtils.Entities.Core;
using OxDb.DataUtils.Importers.Core;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Entities.Interfaces;
using OxDb.SharedCore.Entities.Services;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Riddles.Settings;
using OxDb.SharedGame.Zones.Settings;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;

namespace OxDb.DataUtils.Importers.Zones
{

    public class ZoneTypeImportRow
    {
        public long ZoneTypeId { get; set; }
        public double LargePropChance { get; set; }
        public double SmallPropChance { get; set; }
        public int MaxSmallPropQuantity { get; set; }
        public string Trees { get; set; }
        public string Bushes { get; set; }
        public string Rocks { get; set; }
        public string Props { get; set; }
        public bool IsOutdoors { get; set; }
        public bool IsDungeon { get; set; }


        public class ZoneTypeSettingsImporter : BaseRawDataImporter
        {
            private IEntityService _entityService = null;
            public override string ImportDataFilename => "ZoneTypeSettingsImport.csv";

            public override Type HelperKey => typeof(ZoneType);

            protected override async Task<bool> ParseInputFromLines(EditorGameState gs, List<string[]> lines)
            {

                ZoneTypeSettings settings = gs.data.Get<ZoneTypeSettings>(null);


                gs.LookedAtObjects.Add(settings);

                IReadOnlyList<ZoneType> zoneTypes = settings.GetData();

                foreach (ZoneType zoneType in zoneTypes)
                {
                    zoneType.Props = new List<WeightedEntity>();
                    gs.LookedAtObjects.Add(zoneType);
                }

                ZoneType currZoneType = null;

                string parentName = typeof(ZoneType).Name.ToLower();

                Dictionary<string, string[]> headers = new Dictionary<string, string[]>();

                Dictionary<string, object> _contextObjects = new Dictionary<string, object>();

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

                    if (rowWords[0] == parentName)
                    {
                        ZoneTypeImportRow importRow = _importService.ImportLine<ZoneTypeImportRow>(gs, row, headers[parentName], rowWords);

                        currZoneType = zoneTypes.FirstOrDefault(x => x.IdKey == importRow.ZoneTypeId);

                        if (currZoneType == null)
                        {
                            continue;
                        }

                        currZoneType.LargePropChance = importRow.LargePropChance;
                        currZoneType.SmallPropChance = importRow.SmallPropChance;
                        currZoneType.MaxSmallPropQuantity = importRow.MaxSmallPropQuantity;
                        currZoneType.IsDungeon = importRow.IsDungeon;
                        currZoneType.IsOutdoors = importRow.IsOutdoors;

                        ImportEntityTypes(gs, row,  currZoneType, importRow.Trees, EntityTypes.Tree);
                        ImportEntityTypes(gs, row, currZoneType, importRow.Bushes, EntityTypes.Bush);
                        ImportEntityTypes(gs, row, currZoneType, importRow.Rocks, EntityTypes.Rock);
                        ImportEntityTypes(gs, row, currZoneType, importRow.Props, EntityTypes.Prop);

                    }
                }
                await Task.CompletedTask;
                return true;
            }

            private void ImportEntityTypes(EditorGameState gs, int row, ZoneType ztype, string text, long entityTypeId)
            {

                if (string.IsNullOrEmpty(text))
                {
                    return;
                }

                IEntityHelper helper = _entityService.GetEntityHelper(entityTypeId);


                List<IIdName> children = helper.GetChildList(null);

                string[] words = text.Trim().Split(' ');    

                for (int w =0; w < words.Length; w++)
                {
                    string normWord = StrUtils.NormalizeWord(words[w]);
                    IIdName child = children.FirstOrDefault(x => StrUtils.NormalizeWord(x.Name) == normWord);

                    if (child == null)
                    {
                        throw new Exception("Missing entity named: " + words[w] + " at line " + row);
                    }

                    ztype.Props.Add(new WeightedEntity() { Weight = 1, EntityTypeId = entityTypeId, EntityId = child.IdKey });
                }
            }
        }
    }
}
