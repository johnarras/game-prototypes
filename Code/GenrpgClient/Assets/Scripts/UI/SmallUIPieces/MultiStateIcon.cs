using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Inventory.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.SmallUIPieces
{
    public class MultiStateIcon : BaseBehaviour
    {

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
                _assetService.LoadAtlasSpriteInto(AtlasName, ItemConstants.BlankIconName, Icon, GetToken());
                _clientEntityService.SetActive(Icon, false);
            }
            else
            {
                _assetService.LoadAtlasSpriteInto(AtlasName, StateIconNames[state], Icon, GetToken());
                _clientEntityService.SetActive(Icon, true);
            }
        }
    }
}
