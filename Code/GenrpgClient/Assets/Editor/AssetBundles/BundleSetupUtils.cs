using Assets.Scripts.Assets.Bundles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;


public delegate bool ExtraPrefabSetupStep(GameObject go);

public class BundleSetupUtils
{
    protected static UnityAssetService _assetService;
    public const bool MakeStaticObjects = false;

    public const int BundleFiles = 0;
    public const int BundleDirectories = 1;
    public const int BundleRoot = 2;
    /// <summary>
    /// Use this function to 
    /// </summary>
    /// <param name="path"></param>
    public static void BundleFilesInDirectory(BundleList list, string assetPathSuffix, bool allowAllFiles)
    {
        if (_assetService == null)
        {
            _assetService = new UnityAssetService();
        }

        string endOfPath = _assetService.GetAssetPath(assetPathSuffix);

        string pathWithoutSlash = endOfPath.Replace("/", "");

        List<string> paths = new List<string>();

        string fullPath = AssetConstants.DownloadAssetRootPath + endOfPath;

        string[] files = Directory.GetFiles(fullPath);

        int numAdded = 0;

        foreach (string fileName in files)
        {
            if (SetupFileAtPath(list, assetPathSuffix, fileName, false))
            {
                numAdded++;
            }
        }
        if (numAdded > 0)
        {
            AssetDatabase.SaveAssets();
        }

        foreach (string path in paths)
        {
            if (!Directory.Exists(path))
            {
                continue;
            }
        }

        string[] directories = Directory.GetDirectories(fullPath);

        foreach (string directory in directories)
        {
            string subdirectory = directory.Replace(fullPath, "");
            if (string.IsNullOrEmpty(assetPathSuffix))
            {
                BundleFilesInDirectory(list, assetPathSuffix + (!string.IsNullOrEmpty(assetPathSuffix) ? "/" : "") + subdirectory, false);
            }
            else
            {
                SetupFileAtPath(list, assetPathSuffix, directory, true);
                string bundleName = _assetService.GetBundleNameForCategoryAndAsset(assetPathSuffix, subdirectory);
            }
        }
    }


    private static bool SetupFileAtPath(BundleList list, string assetPathSuffix, string item, bool allowDirectories, string assetBundleName = null)
    {
        if (!allowDirectories && EditorAssetUtils.IsNotPrefabName(item))
        {
            return false;
        }

        if (EditorAssetUtils.IsIgnoreFilename(item))
        {
            return false;
        }

        //AssetDatabase.ImportAsset(item, ImportAssetOptions.Default);

        string fileName = _assetService.StripPathPrefix(item);

        AssetImporter importer = AssetImporter.GetAtPath(item) as AssetImporter;
        if (importer != null)
        {

            string shortFilename = fileName.Replace(AssetConstants.ArtFileSuffix, "");

            string bundleName = assetBundleName;

            if (string.IsNullOrEmpty(bundleName))
            {
                bundleName = _assetService.GetBundleNameForCategoryAndAsset(assetPathSuffix, shortFilename);
            }

            string oldBundleName = importer.assetBundleName;
            importer.assetBundleName = bundleName;

            BundleInfo blitem = list.Bundles.FirstOrDefault(x=>x.BundleName == bundleName);

            if (blitem == null)
            {
                blitem = new BundleInfo() { BundleName = bundleName };

                list.Bundles.Add(blitem);
            }

            if (oldBundleName != bundleName)
            {
                importer.SaveAndReimport();
            }
        }
        return true;
    }




}

