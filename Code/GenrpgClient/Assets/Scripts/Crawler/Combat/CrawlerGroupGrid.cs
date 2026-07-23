using OxDb.Client.Awaitables;
using OxDb.Client.Crawler.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.Combat.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.UnitEffects.Constants;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace OxDb.Client.Crawler.Combat
{
    public class CrawlerGroupGrid : BaseBehaviour
    {

        private IAwaitableService _awaitableService = null;

        public GameObject Anchor;

        public List<CrawlerCombatIcon> Icons = new List<CrawlerCombatIcon>();


        public CrawlerCombatIcon IconTemplate;


        public void Clear()
        {
            _clientEntityService.DestroyAllChildren(Anchor);
            Icons.Clear();
        }

        public void UpdateGroups(List<CombatGroup> groups)
        {

            foreach (CombatGroup group in groups)
            {

                if (group.Units.FastAny(x => x is PartyMember member))
                {
                    continue;
                }

                CrawlerCombatIcon icon = Icons.FirstOrDefault(x => x.Group.Id == group.Id);

                if (!group.Units.FastAny(x => !x.StatusEffects.HasBitIndex(StatusEffects.Dead)))
                {
                    if (icon != null)
                    {
                        Icons.Remove(icon);

                        _awaitableService.ForgetAwaitable(DelayDestroyIcon(icon, icon.GetToken()));
                    }
                }
                else
                {
                    if (icon == null)
                    {
                        icon = _clientEntityService.FullInstantiate(IconTemplate);
                        icon.Group = group;
                        _clientEntityService.AddToParent(icon.gameObject, Anchor);
                        Icons.Add(icon);
                    }

                    icon.UpdateData();
                }
            }

            List<CrawlerCombatIcon> iconsToRemove = new List<CrawlerCombatIcon>();
            foreach (CrawlerCombatIcon icon in Icons)
            {
                CombatGroup currGroup = groups.FirstOrDefault(x => x.Id == icon.Group.Id);

                if (currGroup == null)
                {
                    iconsToRemove.Add(icon);
                }
            }

            foreach (CrawlerCombatIcon icon in iconsToRemove)
            {
                _clientEntityService.Destroy(icon);
                Icons.Remove(icon);
            }
        }


        private async Awaitable DelayDestroyIcon(CrawlerCombatIcon icon, CancellationToken token)
        {
            await Awaitable.WaitForSecondsAsync(CrawlerClientCombatConstants.DestroyCombatIconDelaySeconds, token);
            _clientEntityService.Destroy(icon.gameObject);
        }
    }
}


