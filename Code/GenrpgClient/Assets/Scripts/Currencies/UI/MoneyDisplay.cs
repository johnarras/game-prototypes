using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Currencies.Constants;
using OxDb.SharedGame.Currencies.PlayerData;
using OxDb.SharedGame.Rewards.Messages;
using System.Collections.Generic;

public class MoneyDisplay : BaseBehaviour
{

    public List<MoneySegment> _segments;


    public bool UpdateToCharMoney = false;

    public override void Init()
    {
        base.Init();

        if (UpdateToCharMoney)
        {
            AddListener<OnAddQuantityReward>(OnCurrencyUpdate);
            UpdateValue();
        }
    }

    private void OnCurrencyUpdate(OnAddQuantityReward data)
    {

        if (data.EntityTypeId == EntityTypes.CharCurrency && data.EntityId == CharCurrencyTypes.Money)
        {
            UpdateValue();
        }
        return;
    }

    protected void UpdateValue()
    {
        if (UpdateToCharMoney)
        {
            if (_gs.ch != null)
            {
                CharCurrencyData currencies = _gs.ch.Get<CharCurrencyData>();
                SetMoney(currencies.Data[CharCurrencyTypes.Money]);
            }
        }
    }

    private const int SegmentDiv = 100;

    private long _money = -1; // Force at least one update.
    public void SetMoney(long money)
    {
        if (money < 0)
        {
            money = 0;
        }

        _money = money;
        if (_segments == null || _segments.Count < 1)
        {
            return;
        }

        long amountLeft = _money;
        for (int s = 0; s < _segments.Count; s++)
        {
            MoneySegment seg = _segments[s];

            long currAmount = amountLeft % SegmentDiv;
            if (currAmount == 0 && (money > 0 || s < _segments.Count - 1))
            {
                _clientEntityService.SetActive(seg.GetParent(), false);
            }
            else
            {
                _clientEntityService.SetActive(seg.GetParent(), true);
                seg.SetQuantityText(currAmount.ToString());
            }
            amountLeft /= SegmentDiv;
        }
    }
}

