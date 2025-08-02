using Genrpg.Shared.Crawler.Parties.PlayerData;
using UnityEngine;

namespace Assets.Scripts.Crawler.UI.WorldUI
{
    public class WorldPanelCompass : PartyBuffUI
    { 
        public GImage CompassDirection;

        protected override void FrameUpdateInternal(PartyData party)
        { 
            RectTransform rectTransform = CompassDirection.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                int mapRot = party.CurrPos.Rot;
                if (mapRot % 180 == 0)
                {
                    mapRot += 90;
                }
                else
                {
                    mapRot -= 90;
                }
                rectTransform.localEulerAngles = new Vector3(0, 0, mapRot);
            }
        }
    }
}
