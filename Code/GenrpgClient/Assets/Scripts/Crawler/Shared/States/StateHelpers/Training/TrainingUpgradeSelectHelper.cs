using Genrpg.Shared.Crawler.States.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.Shared.States.StateHelpers.Training
{
    public class TrainingUpgradeSelectHelper : BaseTrainingSelectMemberHelper
    {
        public override ECrawlerStates Key => ECrawlerStates.TrainingUpgradeSelect;

        public override string GetMainMessage() { return "Which party member will get some upgrades?"; }

        public override ECrawlerStates GetNextState() { return ECrawlerStates.TrainingUpgradeMember; }
    }
}
