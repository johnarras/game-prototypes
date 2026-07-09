using OxDb.DataUtils.Entities.Core;
using OxDb.DataUtils.Importers.Core;
using OxDb.SharedGame.Crawler.MapGen.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace OxDb.DataUtils.Importers.Crawler
{
    public class RoomEdgeTypeImporter : ParentChildImporter<RoomEdgeTypeSettings, RoomEdgeType>
    {
        protected override void ImportSubobject(EditorGameState gs, RoomEdgeTypeSettings settings, RoomEdgeType current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
            if (firstColumn == typeof(EdgePattern).Name.ToLower())
            {
                EdgePattern newEdge = _importService.ImportLine<EdgePattern>(gs, row, headers, rowWords);

                EdgePattern? currEdge = settings.EdgePatterns.FirstOrDefault(x=>x.IdKey == newEdge.IdKey);

                if (currEdge != null)
                {
                    settings.EdgePatterns.Remove(currEdge);
                }
                settings.EdgePatterns.Add(newEdge);

                settings.EdgePatterns = settings.EdgePatterns.OrderBy(x=>x.IdKey).ToList();
            }
        }
    }
}
