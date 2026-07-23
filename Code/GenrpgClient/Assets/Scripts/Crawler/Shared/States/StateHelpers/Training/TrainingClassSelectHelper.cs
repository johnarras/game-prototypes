using OxDb.SharedGame.Crawler.States.Constants;

namespace OxDb.Client.Crawler.Shared.States.StateHelpers.Training
{
    public class TrainingClassSelectHelper : BaseTrainingSelectMemberHelper
    {
        public override ECrawlerStates HelperKey => ECrawlerStates.TrainingClassSelect;

        public override string GetMainMessage() { return "Which party member will add a new class?"; }

        public override ECrawlerStates GetNextState() { return ECrawlerStates.TrainingClassMember; }
    }
}


