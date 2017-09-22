using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJson;
using UnityEngine;

namespace Monster.BaseSystem.ResourceManager
{
    public static class BundleTool
    {
        public static string GetAssetName(string path)
        {
            int index = path.LastIndexOf('/');
            return index < 0 ? path : path.Substring(index + 1);
        }

        public static string GetBundleFileName(string assetName)
        {
            return assetName + BundleConfig.FILE_EXTENSION;
        }

        public static string GetBundleFilePath(string assetName)
        {
            return BundleConfig.LOCALE_BUNDLE_FLODER_PATH + "/" + GetBundleFileName(assetName);
        }
    }

    public static class BundleConfig
    {
        public const string CONFIG_FILE_NAME = "AssetsConfig" + FILE_EXTENSION;

        public const string FILE_EXTENSION = ".bytes";

        public static string BUNDLE_REMOTE_PATH = Application.dataPath.Replace("Assets", "") + "Bundles";

        public static string LOCALE_BUNDLE_FLODER_PATH = Application.streamingAssetsPath;

    }

    public class AssetBundleInfoNode
    {
        public List<string> depenceList = new List<string>();
        public string assetName;
    }
}
