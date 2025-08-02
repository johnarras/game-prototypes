
using Assets.Scripts.UI.Constants;
using Assets.Scripts.UI.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.UI.Constants;
using Genrpg.Shared.UI.Settings;
using System;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.UI.Config
{
    [Serializable]
    public class ScreenConfig : TypedEntityIdDropdownScript<ScreenNameSettings,ScreenName>
    {
        public override bool OrderByName() { return true; }

        public ScreenLayers ScreenLayer;
        public string Subdirectory;
#if UNITY_EDITOR
        [MenuItem("Assets/Create/ScriptableObjects/ScreenConfig", false, 0)]
        public static void Create()
        {
            ScriptableObjectUtils.CreateBasicInstance<ScreenConfig>();
        }
#endif
    }
}