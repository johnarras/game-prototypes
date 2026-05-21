using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.UI.Entities
{
    public abstract class EntityIdDropdownList : ScriptableObject
    {
        [HideInInspector]
        public int EntityId;

        public abstract bool OrderByName();
        public abstract List<IIdName> GetChildList(IGameData gameData);

    }
}



