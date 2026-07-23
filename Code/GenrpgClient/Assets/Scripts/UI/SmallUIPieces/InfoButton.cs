using OxDb.Client.UI.Interfaces;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedGame.Ftue.Constants;
using OxDb.SharedGame.Ftue.Services;
using OxDb.SharedGame.Ftue.Settings.Steps;

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

                _ = _ftueService.ForceStartStep(_gs.ch, step.IdKey);

                _logService.Info("Show Info for " + _screenName);
            }
        }
    }
}


