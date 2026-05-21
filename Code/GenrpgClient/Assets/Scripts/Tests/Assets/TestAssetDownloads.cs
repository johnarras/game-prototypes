using Assets.Scripts.Assets.Constants;
using Assets.Scripts.GameObjects;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.GameSettings.Interfaces;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedGame.ProcGen.Settings.Textures;
using OxDb.SharedGame.ProcGen.Settings.Trees;
using OxDb.SharedGame.UI.Settings;
using OxDb.SharedGame.Units.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class TestAssetDownloads : IInjectable
{
    private ILogService _logService = null;
    private IAssetService _assetService = null;
    private IScreenService _screenService = null;
    private IGameData _gameData;
    protected IClientEntityService _clientEntityService = null;
    public async Awaitable RunTests(IClientGameState gs, CancellationToken token)
    {
        gs.loc.Resolve(this);

        _logService.Info("Start test");

        _logService.Info("Test screens");


        TestScreens(token);


        TestAssetCategory<UnitTypeSettings, UnitType>(AssetCategoryNames.Monsters, token);

        TestAssetCategory<TextureTypeSettings, TextureType>(AssetCategoryNames.TerrainTex, token);

        TestAssetCategory<TreeTypeSettings, TreeType>(AssetCategoryNames.Trees, token,
            x => !x.HasFlag(TreeFlags.IsBush));

        TestAssetCategory<TreeTypeSettings, TreeType>(AssetCategoryNames.Bushes, token,
            x => x.HasFlag(TreeFlags.IsBush));

        _logService.Info("Download Tests Complete");

        await Task.CompletedTask;
    }

    private void OnDownloadAsset(GameObject go, object data, CancellationToken token)
    {
        if (go == null)
        {
            _logService.Info("Failed Download: " + data);
        }
        _clientEntityService.Destroy(go);
    }

    private void TestAssetCategory<Parent, Child>(string assetCategoryName, CancellationToken token, Func<Child, bool> filter = null) where Parent : ITopLevelSettings
    {
        Parent settings = _gameData.Get<Parent>(null);

        if (settings == null)
        {
            _logService.Info("Missing settings of type " + typeof(Parent).Name);
            return;
        }

        List<Child> childSettings = settings.GetChildren().Cast<Child>().ToList();

        if (childSettings == null || childSettings.Count < 1)
        {
            return;
        }

        if (filter != null)
        {
            childSettings = childSettings.Where(x => filter(x) == true).ToList();
        }

        foreach (Child setting in childSettings)
        {
            if (setting is IIndexedGameItem indexedItem)
            {
                if (indexedItem.IdKey == 0 ||
                    string.IsNullOrEmpty(indexedItem.Art) ||
                    indexedItem.Art.IndexOf("Unused") >= 0)
                {
                    continue;
                }

                if (filter != null && !filter(setting))
                {
                    continue;
                }

                if (indexedItem is IVariationIndexedGameItem variationItem)
                {
                    for (int i = 1; i <= variationItem.VariationCount; i++)
                    {

                        _assetService.LoadAsset<object>(assetCategoryName, indexedItem.Art + i,
                            OnDownloadAsset, assetCategoryName + "-" + indexedItem.Art + i, token);
                    }
                }

                else
                {
                    _assetService.LoadAsset<object>(assetCategoryName, indexedItem.Art,
                        OnDownloadAsset, assetCategoryName + "-" + indexedItem.Art, token);
                }
            }
        }
    }

    private void TestScreens(CancellationToken token)
    {

        List<ScreenName> snames = _gameData.Get<ScreenNameSettings>(null).GetData().ToList();
        foreach (ScreenName sname in snames)
        {
            string subDir = _screenService.GetSubdirectory(sname.IdKey);

            _assetService.LoadAsset(AssetCategoryNames.UI, sname.Name + "Screen", OnDownloadAsset, "Screen: " + sname.Name, token, default(object), subDir);
        }
    }

}


