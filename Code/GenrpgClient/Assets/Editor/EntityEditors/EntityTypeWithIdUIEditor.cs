
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Entities.Services;
using Genrpg.Shared.Entities.Settings;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

[CustomEditor(typeof(EntityTypeWithIdUI))]
public class EntityTypeWithIdUIEditor : Editor
{

    private int oldSelectedTypeIndex = 0;
    private int oldSelectedEntityIndex = 0;
    private int selectedTypeIndex = 0;
    private int selectedEntityIndex = 0;
    private List<EntityType> entityTypes = new List<EntityType>();
    private List<IIdName> entities = new List<IIdName>();
    string[] entityTypeNames = new string[0];
    public override void OnInspectorGUI()
    {
        bool needToSetDirty = false;
        IClientGameState gs = EditorGameDataUtils.GetEditorGameState();
        EntityTypeWithIdUI entityui = (EntityTypeWithIdUI)target;

        

        IGameData gameData = gs.loc.Get<IGameData>();

        if (entityTypes.Count < 1)
        {

            IReadOnlyList<EntityType> allEntityTypes = gameData.Get<EntitySettings>(null).GetData();

            entityTypes = new List<EntityType>();

            IEntityService entityService = gs.loc.Get<IEntityService>();
            foreach (EntityType etype in allEntityTypes)
            {
                IEntityHelper helper = entityService.GetEntityHelper(etype.IdKey);

                if (helper == null || helper.IsMapEntity())
                {
                    continue;
                }
                entityTypes.Add(etype);
            }

            entityTypes.OrderBy(x => x.Name).ToList();
            entityTypeNames = entityTypes.ConvertAll(x => x.Name + " (" + x.IdKey + ")").ToArray();

            for (int i = 0; i < entityTypes.Count; i++)
            {
                if (entityui.EntityTypeId == entityTypes[i].IdKey)
                {
                    selectedTypeIndex = i;
                    break;
                }
            }
        }
        selectedTypeIndex = EditorGUILayout.Popup("Select EntityType: ", selectedTypeIndex, entityTypeNames);
      
        if (selectedTypeIndex >= 0 && selectedTypeIndex < entityTypes.Count)
        {
            entityui.EntityTypeId = (int)(entityTypes[selectedTypeIndex].IdKey);
        }

        if (selectedTypeIndex != oldSelectedTypeIndex)
        {
            needToSetDirty = true;
            oldSelectedTypeIndex = selectedTypeIndex;
        }


        entities = EditorGameDataUtils.GetEntityListForEntityTypeId(entityui.EntityTypeId);
        string[] entityNames = entities.ConvertAll(x => x.Name + " (" + x.IdKey+ ")").ToArray();

        EntityType currEntityType = entityTypes.FirstOrDefault(x=>x.IdKey == entityui.EntityTypeId);

        for (int i = 0; i < entities.Count; i++)
        {
            if (entityui.EntityId == entities[i].IdKey)
            {
                selectedEntityIndex = i;
                break;
            }
        }

        selectedEntityIndex = EditorGUILayout.Popup("Select " + currEntityType.Name, selectedEntityIndex, entityNames);

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
    }
}
