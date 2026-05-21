using Assets.Scripts.Assets.Constants;
using Assets.Scripts.Assets.Sprites.Services;

namespace Assets.Scripts.CombatFX
{
    public class CombatBolt : BaseBehaviour
    {
        protected ISpriteService _spriteService = null;

        public GImage Image;

        public void InitElementImage(string imageName)
        {
            _spriteService.SetAtlasSpriteInto(AtlasNames.CrawlerCombat, imageName + "Bolt", Image, GetToken());
        }
    }
}


