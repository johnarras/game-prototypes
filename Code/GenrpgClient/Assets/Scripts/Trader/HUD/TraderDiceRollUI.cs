using OxDb.Client.Trader.HUD.ClientEvents;
using OxDb.SharedCore.Utils;
using System.Text;
using System.Threading;
using UnityEngine;

namespace OxDb.Client.Trader.HUD
{
    public class TraderDiceRollUI : BaseBehaviour
    {
        public GameObject TextParent;
        public GText Text;
        public float VisibleTime = 0.5f;

        public override void Init()
        {
            _dispatcher.AddListener<ShowTraderDiceRoll>(OnShowTraderDiceRoll, GetToken());
        }

        private void OnShowTraderDiceRoll(ShowTraderDiceRoll diceRoll)
        {
            StringBuilder sb = new StringBuilder();

            for (int r = 0; r < diceRoll.RolledDistances.Count; r++)
            {
                long diceVal = MathUtil.Clamp(1, diceRoll.RolledDistances[r], 6);
                sb.Append($"<sprite name=\"Die{diceVal}\">");
                if (r < diceRoll.RolledDistances.Count - 1)
                {
                    sb.Append(" + ");
                }
            }

            if (diceRoll.BonusDistance > 0)
            {
                sb.Append(" + " + diceRoll.BonusDistance);
            }

            sb.Append(" = " + diceRoll.TotalDistance);

            _clientEntityService.SetActive(TextParent, true);
            _uiService.SetText(Text, sb.ToString());

            _updateService.AddDelayedUpdate(this, HideText, VisibleTime, GetToken());


        }

        private void HideText(CancellationToken token)
        {
            _clientEntityService.SetActive(TextParent, false);
        }
    }
}