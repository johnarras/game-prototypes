using Assets.Scripts.Entities.UI;
using Assets.Scripts.UI.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.UI.Entities
{
    public class RowEntityIcon : EntityIcon
    {
        protected ITextService _textService = null;
        public override void SetEntityData(long entityTypeId, long entityId, long quantity, long maxQuantity = 0)
        {
            base.SetEntityData(entityTypeId, entityId, quantity, maxQuantity);
            if (quantity >= 0)
            {
                _uiService.SetText(QuantityText, "+" +  quantity);
                _uiService.SetColor(QuantityText, Color.white);
            }
            else
            {
                _uiService.SetColor(QuantityText, Color.red);
            }
        }
    }
}
