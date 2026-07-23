using ClientEvents;
using OxDb.Client.ClientEvents.UI;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.UI.Constants;
using System.Threading;
using System.Threading.Tasks;

public interface IPopupManager : IInitializable
{
}

public class PopupManager : IPopupManager
{
    protected IDispatcher _dispatcher = null;

    public async Task Initialize(CancellationToken token)
    {
        _dispatcher.AddListener<ShowLootEvent>(OnLootPopup, token);
        await Task.CompletedTask;
    }

    private void OnLootPopup(ShowLootEvent ldata)
    {
        if (ldata == null || ldata.Rewards == null || ldata.Rewards.Count < 1)
        {
            return;
        }
        _dispatcher.Dispatch(new OpenScreen(ScreenNames.Loot, ldata.Rewards));

    }
}

