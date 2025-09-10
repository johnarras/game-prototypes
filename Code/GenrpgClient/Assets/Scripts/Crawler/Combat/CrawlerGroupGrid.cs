using Assets.Scripts.Awaitables;
using Assets.Scripts.Crawler.Constants;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.UnitEffects.Constants;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Crawler.Combat
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

                if (group.Units.Any(x => x is PartyMember member))
                {
                    continue;
                }

                CrawlerCombatIcon icon = Icons.FirstOrDefault(x => x.Group.Id == group.Id);

                if (!group.Units.Any(x => !x.StatusEffects.HasBit(StatusEffects.Dead)))
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
