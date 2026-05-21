using Assets.Scripts.UI.ScreenSystem;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedGame.Inventory.PlayerData;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class LootPopup : TypedArgScreen<List<RewardList>>
{

    protected IIconService _iconService = null;
    public GameObject _itemAnchor;

    public float _itemDelay = 0.5f;


    public override bool BlockMouse() { return false; }

    protected override async Task OnStartOpen(List<RewardList> rewardLists, CancellationToken token)
    {
        if (rewardLists == null || rewardLists.Count < 1)
        {
            StartClose();
            return;
        }

        List<Reward> rewards = rewardLists.SelectMany(x => x.Rewards).ToList();


        _awaitableService.ForgetAwaitable(ShowRewards(rewards, token));


        await Task.CompletedTask;
    }

    private async Awaitable ShowRewards(List<Reward> rewards, CancellationToken token)
    {
        if (rewards == null || rewards.Count < 1 || _itemAnchor == null)
        {
            StartClose();
            return;
        }

        foreach (Reward rew in rewards)
        {
            InitItemIconData iid = new InitItemIconData()
            {
                Data = rew.ExtraData as Item,
                EntityTypeId = rew.EntityTypeId,
                EntityId = rew.EntityId,
                Quantity = rew.Quantity,
            };
            _iconService.InitItemIcon(iid, _itemAnchor, _assetService, token);
        }

        while (true)
        {
            await Awaitable.WaitForSecondsAsync(_itemDelay, cancellationToken: token);

            if (_itemAnchor.transform.childCount < 1)
            {
                break;
            }
            GameObject firstChild = _itemAnchor.transform.GetChild(0).gameObject;
            _clientEntityService.Destroy(firstChild);
        }
        StartClose();
    }

}

