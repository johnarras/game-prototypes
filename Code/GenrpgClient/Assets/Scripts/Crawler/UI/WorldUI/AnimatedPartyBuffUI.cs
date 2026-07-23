using OxDb.Client.Assets.Textures;
using OxDb.SharedGame.Crawler.Buffs.Settings;

namespace OxDb.Client.Crawler.UI.WorldUI
{
    public class AnimatedPartyBuffUI : PartyBuffUI
    {
        public AnimatedSprite Sprite;

        public override void Init()
        {
            base.Init();


            string imageName = "";
            if (string.IsNullOrEmpty(imageName))
            {
                PartyBuff buff = _gameData.Get<PartyBuffSettings>(_gs.ch).Get(PartyBuffId);

                if (buff != null)
                {
                    imageName = buff.Icon;
                }
            }

            Sprite.SetImage(imageName);
            Sprite.FramesBetweenSequenceStep = (int)UpdateTicks;
        }
    }
}


