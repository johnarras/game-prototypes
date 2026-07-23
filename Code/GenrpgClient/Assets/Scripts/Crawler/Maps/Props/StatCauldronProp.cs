using OxDb.Client.Crawler.Maps.Loading;
using OxDb.Client.ProcGen.Materials;
using OxDb.SharedGame.Stats.Settings.Stats;
using UnityEngine;

namespace OxDb.Client.Crawler.Maps.Props
{
    public class StatCauldronProp : MapProp
    {
        public override void SetData(CrawlerObjectLoadData loadData)
        {
            base.SetData(loadData);

            TriColorRemapMaterial remap = GetComponent<TriColorRemapMaterial>();

            if (remap == null)
            {
                return;
            }

            StatType stype = loadData.Data as StatType;

            if (stype == null)
            {
                return;
            }

            if (ColorUtility.TryParseHtmlString(stype.ColorCode, out Color currColor))
            {

                remap.SetColors(currColor, Color.green, Color.blue);
            }

        }
    }
}
