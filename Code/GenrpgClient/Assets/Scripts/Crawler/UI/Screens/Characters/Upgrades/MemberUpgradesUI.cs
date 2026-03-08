using Genrpg.Shared.Crawler.Crawlers.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Upgrades.Settings;
using Genrpg.Shared.Entities.Constants;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Crawler.UI.Screens.Characters.Upgrades
{
    public class MemberUpgradesUI : BaseBehaviour
    {


        protected ICrawlerUpgradeService _upgradeService = null;

        public MemberUpgradeRow RowPrefab;

        public GameObject ContentRoot;

        public GText PointsLeft;


        public void SetData(PartyMember member)
        {
            IReadOnlyList<MemberUpgrade> upgrades = _gameData.Get<MemberUpgradeSettings>(_gs.ch).GetData();


            _clientEntityService.DestroyAllChildren(ContentRoot);

            _uiService.SetText(PointsLeft, "Upgrade Points: " + member.UpgradePoints.ToString());


            foreach (MemberUpgrade upgrade in upgrades)
            {
                MemberUpgradeRow row = _clientEntityService.FullInstantiate<MemberUpgradeRow>(RowPrefab);

                _clientEntityService.AddToParent(row, ContentRoot);

                row.SetData(upgrade, member.Upgrades[upgrade.IdKey], _upgradeService.GetUnitBonus(member, EntityTypes.MemberUpgrades, upgrade.IdKey));

            }

        }
    }
}


