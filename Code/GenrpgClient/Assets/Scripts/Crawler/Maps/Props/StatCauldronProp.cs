using Assets.Scripts.Crawler.Maps.Loading;
using Assets.Scripts.ProcGen.Materials;
using OxDb.SharedGame.Stats.Settings.Stats;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.Props
{
    public class StatCauldronProp : CrawlerProp
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
