using Assets.Scripts.WorldCanvas.GameEvents;
using Genrpg.Shared.Client.Assets.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.CombatFX
{
    public class CombatBolt : BaseBehaviour
    {
        public GImage Image;

        public void InitElementImage(string imageName)
        {
            _assetService.LoadAtlasSpriteInto(AtlasNames.CrawlerCombat, imageName + "Bolt", Image, GetToken());
        }
    }
}
