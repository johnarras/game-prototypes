using Assets.Scripts.FloatingText.ClientEvents;
using Assets.Scripts.Rewards.Services;
using OxDb.SharedCore.Entities.Interfaces;
using OxDb.SharedCore.Entities.Services;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedGame.Rewards.Constants;
using OxDb.SharedGame.Rewards.Services;
using OxDb.SharedGame.Trader.Encounters.Entities;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Trader.Encounters.Services
{
    public interface IClientEncounterService : IInjectable
    {
        Awaitable ShowEncounterResult(EncounterResult result);
    }

    public class ClientEncounterService : IClientEncounterService
    {

        private IClientGameState _gs = null;
        private IRewardService _rewardService = null;
        private IDispatcher _dispatcher = null;
        private IEntityService _entityService = null;
        public async Awaitable ShowEncounterResult(EncounterResult result)
        {
            await Task.CompletedTask;
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

                    await _rewardService.GiveReward(_gs.ch, rew, RewardSources.TravelEncounter, new ClientRewardParams(false, true));

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
