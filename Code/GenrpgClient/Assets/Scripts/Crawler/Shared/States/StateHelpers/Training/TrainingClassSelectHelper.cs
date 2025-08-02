using Genrpg.Shared.Crawler.States.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.Shared.States.StateHelpers.Training
{
    public class TrainingClassSelectHelper : BaseTrainingSelectMemberHelper
    {
        public override ECrawlerStates Key => ECrawlerStates.TrainingClassSelect;

        public override string GetMainMessage() { return "Which party member will add a new class?"; }

        public override ECrawlerStates GetNextState() { return ECrawlerStates.TrainingClassMember; }
    }
}
