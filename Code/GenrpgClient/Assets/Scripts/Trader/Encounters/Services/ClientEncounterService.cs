using Assets.Scripts.Core;
using Assets.Scripts.DynamicUI.Services;
using Assets.Scripts.FloatingText.ClientEvents;
using Assets.Scripts.Rewards.Services;
using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Entities.Services;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Rewards.Services;
using Genrpg.Shared.Trader.Encounters.Entities;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Trader.Encounters.Services
{
    public interface IClientEncounterService : IInjectable
    {
        Awaitable ShowEncounterResult(EncounterResult result);
    }

    public class ClientEncounterService : IClientEncounterService
    {

        private IGameData _gameData = null;
        private IClientGameState _gs = null;
        private IRewardService _rewardService = null;
        private IDispatcher _dispatcher = null;
        private IClientRandom _rand = null;
        private IDynamicUIService _dynamicUIService = null;
        private IEntityService _entityService = null;
        public async Awaitable ShowEncounterResult(EncounterResult result)
        {
            if (result == null)
            {
                return;
            }

            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(result.Message))
            {
                sb.Append(result.Message);
            }
            List<ShowFloatingText> showTextList = new List<ShowFloatingText>();
            foreach (RewardList rlist in result.RewardLists)
            {
                foreach (Reward rew in rlist.Rewards)
                {

                    await _rewardService.GiveReward(_gs.ch, rew, new ClientRewardParams(false, true));

                    IEntityHelper helper = _entityService.GetEntityHelper(rew.EntityTypeId);
                    if (helper != null)
                    {
                        IIdName idname = helper.Find(_gs.ch, rew.EntityId);

                        if (idname != null)
                        {
                            string plusString = rew.Quantity > 0 ? "+" : "";
                            if (idname != null)
                            {
                                sb.Append("    " + plusString + rew.Quantity + " " + idname.Name);
                            }
                        }
                    }
                }
            }

            if (sb.Length > 0)
            {
                _dispatcher.Dispatch(new ShowFloatingText(sb.ToString(), result.IsBad ? EFloatingTextArt.Error : EFloatingTextArt.Message));
            }
        }
    }
}
