using OxDb.Client.Assets.Constants;
using OxDb.Client.GameObjects;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Inventory.Settings.Qualities;
using System.Threading;
using UnityEngine;

public interface IIconService : IInjectable
{
    void InitItemIcon(InitItemIconData data, GameObject parent, IAssetService assetService, CancellationToken token);
    void InitSpellIcon(InitSpellIconData data, GameObject parent, IAssetService assetService, CancellationToken token);
}

public class IconService : IIconService
{
    private IClientEntityService _clientEntityService = null;

    public const string DefaultItemIconName = "ItemIcon";
    public const string DefaultSpellIconName = "SpellIcon";

    public string GetBackingNameFromQuality(IGameData gameData, long qualityTypeId)
    {
        string txt = "BGCommon";
        QualityType quality = gameData.Get<QualityTypeSettings>(null).Get(qualityTypeId);
        if (quality == null || string.IsNullOrEmpty(quality.Icon))
        {
            return txt;
        }

        return quality.Icon;
    }

    public string GetFrameNameFromLevel(IGameData gameData, long level)
    {
        if (level < 0)
        {
            level = 0;
        }

        if (level > 100)
        {
            level = 100;
        }

        level -= level % 5;
        return "Frame_" + level.ToString().PadLeft(3, '0');
    }


    public void InitItemIcon(InitItemIconData data, GameObject parent, IAssetService assetService, CancellationToken token)
    {
        string prefabName = DefaultItemIconName;

        if (data != null && !string.IsNullOrEmpty(data.IconPrefabName))
        {
            prefabName = data.IconPrefabName;
        }

        assetService.LoadAssetInto(parent, AssetCategoryNames.UI,
            prefabName, OnLoadItemIcon, token, data, data.SubDirectory);

    }

    private void OnLoadItemIcon(GameObject go, InitItemIconData idata, CancellationToken token)
    {
        ItemIcon iicon = go.GetComponent<ItemIcon>();
        if (iicon == null)
        {
            _clientEntityService.Destroy(go);
            return;
        }

        if (idata == null)
        {
            _clientEntityService.Destroy(go);
            return;
        }

        iicon.Init(idata, token);

    }

    public void InitSpellIcon(InitSpellIconData data, GameObject parent, IAssetService assetService, CancellationToken token)
    {
        string prefabName = DefaultSpellIconName;

        if (data != null && !string.IsNullOrEmpty(data.iconPrefabName))
        {
            prefabName = data.iconPrefabName;
        }

        assetService.LoadAssetInto(parent, AssetCategoryNames.UI,
            prefabName, OnLoadSpellIcon, token, data, data.subdirectory);

    }

    private void OnLoadSpellIcon(GameObject go, InitSpellIconData idata, CancellationToken token)
    {
        SpellIcon iicon = go.GetComponent<SpellIcon>();
        if (iicon == null)
        {
            _clientEntityService.Destroy(go);
            return;
        }

        if (idata == null)
        {
            _clientEntityService.Destroy(go);
            return;
        }

        iicon.Init(idata, token);

    }

}


