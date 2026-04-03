using Assets.Scripts.ClientEvents.UI;
using ClientEvents;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.UI.Constants;
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

