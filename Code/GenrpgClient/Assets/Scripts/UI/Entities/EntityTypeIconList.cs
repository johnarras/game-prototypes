
using Assets.Scripts.Assets.Constants;
using Assets.Scripts.Assets.ObjectPools;
using OxDb.SharedCore.Effects.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Entities.UI
{
    public class EntityTypeIconList : BaseBehaviour
    {

        public GameObject IconAnchor;

        public string IconPrefabName;

        public string IconSubdirectory;

        IObjectPool _pool = null;

        private List<EntityIcon> _icons = new List<EntityIcon>();

        public void ShowSmallIdList(long entityTypeId, long[] quantities, long multiplier)
        {

            List<Effect> effects = new List<Effect>();

            for (int i = 0; i < quantities.Length; i++)
            {
                if (quantities[i] != 0)
                {
                    effects.Add(new Effect()
                    {
                        EntityTypeId = entityTypeId,
                        EntityId = i,
                        Quantity = quantities[i],
                    });
                }
            }

            ShowEffectList(effects);
        }


        public void ShowEffectList<T>(List<T> effects) where T : IEffect
        {

            List<IEffect> addList = new List<IEffect>();

            List<EntityIcon> foundIcons = new List<EntityIcon>();

            for (int i = 0; i < effects.Count; i++)
            {
                IEffect eff = effects[i];
                EntityIcon currIcon = _icons.FirstOrDefault(x => x.EntityTypeId == eff.EntityTypeId && x.EntityId == eff.EntityId);

                if (currIcon != null)
                {
                    currIcon.SetEntityData(eff);
                    foundIcons.Add(currIcon);
                    continue;
                }
                else
                {
                    addList.Add(eff);
                }
            }

            List<EntityIcon> removeList = _icons.Except(foundIcons).ToList();

            foreach (EntityIcon icon in removeList)
            {
                _icons.Remove(icon);
                _pool.ReturnObject(icon);
            }

            foreach (IEffect eff in addList)
            {
                _pool.CheckoutObject(IconAnchor, AssetCategoryNames.UI, IconPrefabName, OnLoadIcon, eff, GetToken(), IconSubdirectory);
            }
        }

        private void OnLoadIcon(GameObject go, IEffect rew, CancellationToken token)
        {
            if (go == null)
            {
                return;
            }

            EntityIcon icon = go.GetComponent<EntityIcon>();

            icon.SetEntityData(rew);

            _icons.Add(icon);

            _icons = _icons.OrderBy(x => x.EntityId).ToList();

            _clientEntityService.ReorderSiblings(_icons);
        }
    }
}


