using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Crawler.Crawlers.Services;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Upgrades.Settings;
using System.Collections.Generic;
using UnityEngine;

namespace OxDb.Client.Crawler.UI.Screens.Characters.Upgrades
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


