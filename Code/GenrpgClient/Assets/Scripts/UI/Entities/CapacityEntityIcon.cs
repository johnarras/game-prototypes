using Assets.Scripts.Entities.UI;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.UI.Entities
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
