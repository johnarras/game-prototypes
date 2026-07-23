using OxDb.Client.Assets.Constants;
using OxDb.Client.Assets.Sprites.Services;

namespace OxDb.Client.CombatFX
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


