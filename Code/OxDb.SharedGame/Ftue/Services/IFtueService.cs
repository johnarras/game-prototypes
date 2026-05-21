using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Ftue.Settings.Steps;

namespace OxDb.SharedGame.Ftue.Services
{
    public interface IFtueService : IInjectable
    {
        bool IsComplete(IRandom rand, Character ch);
        FtueStep GetCurrentStep(IRandom rand, Character ch);
        FtueStep StartStep(IRandom rand, Character ch, long ftueStepId);
        bool CanClickButton(IRandom rand, Character ch, string screenName, string buttonName);
        void CompleteStep(IRandom rand, Character ch, long ftueStepId);
    }
}


