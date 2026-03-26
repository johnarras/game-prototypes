using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Ftue.Settings.Steps;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;

namespace Genrpg.Shared.Ftue.Services
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


