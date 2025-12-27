using Assets.Scripts.UI.Interfaces;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Ftue.Constants;
using Genrpg.Shared.Ftue.Services;
using Genrpg.Shared.Ftue.Settings.Steps;

public class InfoButton : BaseBehaviour
{
    private IFtueService _ftueService = null;
    protected IRepositoryService _repoService = null;
    public GButton Button;

    private string _screenName = null;
    public override void Init()
    {
        base.Init();
        IScreen screen = _clientEntityService.FindInParents<IScreen>(gameObject);

        if (screen != null)
        {
            _screenName = screen.GetName();
            _uiService.SetButton(Button, screen.GetName(), ClickInfoButton);
        }
    }

    private void ClickInfoButton()
    {
        // if (_ftueService.GetCurrentStep(_gs,_gs.ch) == null)
        {
            FtueStep step = _gameData.Get<FtueStepSettings>(_gs.ch).FindFtueStep(FtueTriggers.InfoButton, _screenName);

            if (step != null)
            {

                _ftueService.StartStep(_rand, _gs.ch, step.IdKey);

                _logService.Info("Show Info for " + _screenName);
            }
        }
    }
}


