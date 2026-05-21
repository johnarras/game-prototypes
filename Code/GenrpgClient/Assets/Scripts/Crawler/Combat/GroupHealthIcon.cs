using OxDb.SharedGame.Crawler.Monsters.Entities;
using OxDb.SharedGame.Stats.Constants;
using OxDb.SharedGame.UnitEffects.Constants;
using UnityEngine;

namespace Assets.Scripts.Crawler.Combat
{
    public class GroupHealthIcon : BaseBehaviour
    {
        public GameObject ImageObject;
        public GImage Image;


        Color lowColor = Color.red;
        Color midColor = Color.yellow;
        Color highColor = Color.green;

        public void UpdateFromUnit(CrawlerUnit unit)
        {
            if (unit == null || unit.StatusEffects.HasBitIndex(StatusEffects.Dead))
            {
                _clientEntityService.SetActive(ImageObject, false);
            }
            else
            {
                _clientEntityService.SetActive(ImageObject, true);

                float percent = unit.Stats.Curr(StatTypes.Health) * 1.0f / unit.Stats.Max(StatTypes.Health);

                if (percent <= 0.5)
                {
                    percent *= 2;

                    Color currColor = lowColor * (1 - percent) + midColor * percent;

                    Image.SetColor(currColor);

                }
                else
                {
                    percent -= 0.5f;
                    percent *= 2;

                    Color currColor = midColor * (1 - percent) + highColor * percent;
                    Image.SetColor(currColor);
                }
            }
        }
    }
}
