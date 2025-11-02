using Assets.Scripts.Assets.Sprites.Services;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Inventory.Constants;
using System.Collections.Generic;

namespace Assets.Scripts.UI.SmallUIPieces
{
    public class MultiStateIcon : BaseBehaviour
    {
        protected ISpriteService _spriteService = null;

        public string AtlasName = AtlasNames.UI;
        public List<string> StateIconNames = new List<string>();

        public GImage Icon;

        public override void Init()
        {
            base.Init();
            SetState(0);
        }

        public void SetState(int state)
        {
            if (state < 0 || state >= StateIconNames.Count ||
                string.IsNullOrEmpty(StateIconNames[state]) ||
                StateIconNames[state] == ItemConstants.BlankIconName)
            {
                _spriteService.LoadAtlasSpriteInto(AtlasName, ItemConstants.BlankIconName, Icon, GetToken());
                _clientEntityService.SetActive(Icon, false);
            }
            else
            {
                _spriteService.LoadAtlasSpriteInto(AtlasName, StateIconNames[state], Icon, GetToken());
                _clientEntityService.SetActive(Icon, true);
            }
        }
    }
}
