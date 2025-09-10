using Genrpg.Shared.Client.Assets.Constants;

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
