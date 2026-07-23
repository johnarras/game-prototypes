using OxDb.SharedGame.Trader.CurrencySpend.Settings;
using OxDb.SharedGame.Trader.CurrencySpend.WebApi;

namespace OxDb.Client.Trader.Currencies.UI
{
    public class SpendTypeIcon : BaseBehaviour
    {

        public GText Description;
        public SpendCurrencyButton Button;

        protected SpendLocation _loc;
        protected SpendType _spendType;
        protected long _targetEntityId;

        public virtual void SetData(SpendLocation loc, SpendType spendType, long targetEntityId = 0)
        {
            _loc = loc;
            _spendType = spendType;
            _targetEntityId = targetEntityId;

            Button.SetSpendType(loc, spendType, UpdateSpendRequest);

            _uiService.SetText(Description, spendType.Name);
        }

        public long GetSpendTypeIndex()
        {
            return _spendType.Index;
        }

        private bool UpdateSpendRequest(SpendCurrencyRequest request)
        {
            request.TargetEntityId = _targetEntityId;
            return true;
        }
    }
}
