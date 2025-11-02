using Assets.Scripts.Assets.Sprites.Services;
using Genrpg.Shared.Client.Assets.Constants;

namespace Assets.Scripts.CombatFX
{
    public class CombatBolt : BaseBehaviour
    {
        protected ISpriteService _spriteService = null;

        public GImage Image;

        public void InitElementImage(string imageName)
        {
            _spriteService.LoadAtlasSpriteInto(AtlasNames.CrawlerCombat, imageName + "Bolt", Image, GetToken());
        }
    }
}
