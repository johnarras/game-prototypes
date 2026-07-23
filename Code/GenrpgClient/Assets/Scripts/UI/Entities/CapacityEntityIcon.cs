using OxDb.Client.Entities.UI;
using UnityEngine;

namespace OxDb.Client.UI.Entities
{
    public class CapacityEntityIcon : EntityIcon
    {
        public override void SetEntityData(long entityTypeId, long entityId, long quantity, long maxQuantity = 0)
        {
            if (maxQuantity < 1)
            {
                maxQuantity = 1;
            }
            base.SetEntityData(entityTypeId, entityId, quantity, maxQuantity);

            _uiService.SetColor(QuantityText, (quantity <= maxQuantity ? Color.white : Color.red));
        }
    }
}
