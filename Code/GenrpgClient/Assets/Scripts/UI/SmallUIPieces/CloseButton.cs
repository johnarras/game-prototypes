using Assets.Scripts.UI.Interfaces;

public class CloseButton : BaseBehaviour
{
    public GButton Button;

    public override void Init()
    {
        base.Init();
        IScreen screen = _clientEntityService.FindInParents<IScreen>(gameObject);

        if (screen != null)
        {
            _uiService.SetButton(Button, screen.GetName(), screen.StartClose);
        }
    }
}


