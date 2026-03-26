using Genrpg.Shared.Effects.Entities;
using Genrpg.Shared.Trader.CurrencySpend.Settings;

namespace Assets.Scripts.Trader.Currencies.UI
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

            Button.SetSpendType(loc, spendType, targetEntityId);

            _uiService.SetText(Description, spendType.Name);
        }

        public long GetSpendTypeIndex()
        {
            return _spendType.Index;
        }
    }
}
