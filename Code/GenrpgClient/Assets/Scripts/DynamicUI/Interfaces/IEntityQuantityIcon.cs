using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.DynamicUI.Interfaces
{
    public interface IEntityQuantityIcon
    {
        public void AddVisualQuantity(long entityTypeId, long entityId, long quantityAdded, bool instant);
    }
}
