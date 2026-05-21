
using ClientEvents;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Currencies.Constants;
using OxDb.SharedGame.Currencies.PlayerData;
using OxDb.SharedGame.Rewards.Messages;
using OxDb.SharedGame.RpgLevels.Settings;
using OxDb.SharedGame.Units.Entities;

public class ExpBar : BaseBehaviour
{
    public ProgressBar _progressBar;

    private long _curr = 0;
    private long _max = 0;
    private Unit _unit = null;

    public void Init(Unit unitIn)
    {

        AddListener<LevelUpEvent>(OnLevelUpdate);
        AddListener<OnAddQuantityReward>(OnAddQuantityRewardHandler);
        _unit = unitIn;

        long currLevelId = _gs.ch.Level;

        RpgLevel nextLevelData = _gameData.Get<RpgLevelSettings>(_gs.ch).Get(_gs.ch.Level);

        if (nextLevelData == null)
        {
            return;
        }

        CharCurrencyData currencies = _gs.ch.Get<CharCurrencyData>();

        long currExp = currencies.Data[CharCurrencyTypes.Exp];

        if (_progressBar != null && _unit != null)
        {
            _curr = currExp;
            _max = nextLevelData.CurrExp;
            _progressBar.InitRange(0, _curr, _max);
        }
    }

    private void OnAddQuantityRewardHandler(OnAddQuantityReward data)
    {
        if (data.EntityTypeId == EntityTypes.CharCurrency && data.EntityId == CharCurrencyTypes.Money)
        {
            Init(_unit);
        }
        return;
    }

    private void OnLevelUpdate(LevelUpEvent data)
    {
        Init(_unit);
        return;
    }
}

