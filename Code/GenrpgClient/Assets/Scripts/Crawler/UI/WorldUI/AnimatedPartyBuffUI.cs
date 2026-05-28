using Assets.Scripts.Assets.Textures;

namespace Assets.Scripts.Crawler.UI.WorldUI
{
    public class AnimatedPartyBuffUI : PartyBuffUI
    {
        public string ImageName;
        public AnimatedSprite Sprite;

        public override void Init()
        {
            base.Init();

            Sprite.SetImage(ImageName);
            Sprite.FramesBetweenSequenceStep = (int)UpdateTicks;
        }
    }
}


