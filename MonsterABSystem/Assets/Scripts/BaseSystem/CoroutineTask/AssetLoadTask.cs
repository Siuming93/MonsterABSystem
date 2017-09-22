
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LitJson;
using Monster.BaseSystem.ResourceManager;
using UnityEngine;

namespace Monster.BaseSystem.CoroutineTask
{
    public class AssetLoadTask: BaseCoroutineTask
    {
        public override IEnumerator Run()
        {
            Init();
            yield return 1;
            DownLoadBundles();
            this.Progress = 1;
            this.IsCompleted = true;
        }

        public override void Dispose()
        {
        }
#region self funcs

        private void Init()
        {
            this.Progress = 0f;
            this.Description = "加载AssetBundles";
        }

        private void DownLoadBundles()
        {
            var targetPath = Application.streamingAssetsPath;
            var map = FindAllBundleNames();
            AssetBundleConfig.map = map;
            foreach (var name in map.Keys)
            {
                var bundleName = BundleTool.GetBundleFileName(name);
                File.Copy(BundleConfig.BUNDLE_REMOTE_PATH + "/" + bundleName, targetPath + "/" + bundleName, true);
            }
        }

        private Dictionary<string, AssetBundleInfoNode> FindAllBundleNames()
        {
#if !UNITY_EDITOR
            return new Dictionary<string, AssetRefrenceNode>();

#endif
            var reader = new StreamReader(File.OpenRead(BundleConfig.BUNDLE_REMOTE_PATH + "/" + BundleConfig.CONFIG_FILE_NAME));
            var config = reader.ReadToEnd();
            reader.Dispose();
            var map = JsonMapper.ToObject<Dictionary<string, AssetBundleInfoNode>>(config);
            return map;
        }
#endregion
    }
}
