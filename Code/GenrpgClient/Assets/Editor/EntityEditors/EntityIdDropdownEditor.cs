
using Assets.Scripts.UI.Entities;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

[CustomEditor(typeof(EntityIdDropdownList), true)]
public class EntityIdDropdownEditor : Editor
{
    
    private int oldSelectedEntityIndex = 0;
    private int selectedEntityIndex = 0;
    private List<IIdName> entities = new List<IIdName>();
    string[] entityNames = new string[0];   
    public override void OnInspectorGUI()
    {
        bool needToSetDirty = false;
        IClientGameState gs = EditorGameDataUtils.GetEditorGameState();
        EntityIdDropdownList entityui = (EntityIdDropdownList)target;

        IGameData gameData = gs.loc.Get<IGameData>();

        if (entities.Count < 1)
        {
            entities = entityui.GetChildList(gameData);

            if (entityui.OrderByName())
            {
                entities = entities.OrderBy(e => e.Name).ToList();
            }

            entityNames = entities.ConvertAll(x => x.Name + " (" + x.IdKey + ")").ToArray();

            for (int i = 0; i < entities.Count; i++)
            {
                if (entityui.EntityId == entities[i].IdKey)
                {
                    selectedEntityIndex = i;
                    break;
                }
            }
        }

        selectedEntityIndex = EditorGUILayout.Popup("Select Item: ", selectedEntityIndex, entityNames);

        if (selectedEntityIndex >= 0 && selectedEntityIndex < entities.Count)
        {
            entityui.EntityId = (int)entities[selectedEntityIndex].IdKey;
        }

        if (selectedEntityIndex != oldSelectedEntityIndex)
        {
            needToSetDirty = true;
            oldSelectedEntityIndex = selectedEntityIndex;
        }

        if (needToSetDirty)
        {
            EditorUtility.SetDirty(target);
        }

        DrawDefaultInspector();
    }
}
