using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Monster.BaseSystem.ResourceManager
{
    public class BundleResourceManager : BaseResourceManager
    {
        public override void Init(object data)
        {
            base.Init(data);
            HintInit();
        }

        protected override Object DoLoad(string path, Type type, bool instantiate)
        {
            AssetBundleHint hint = HintGet(path);
            if (hint != null)
            {
                return HintLoadMainAsset(hint, instantiate);
            }
            else
            {
                Debug.LogError("没有Asset资源");
                return null;
            }
        }

        protected override T DoLoad<T>(string path)
        {
            return DoLoad(path, typeof(T), false) as T;
        }

        protected override void DoLoadAsync(string path, Type type, ResourceAsyncCallBack callBack)
        {
            DoLoadAsync(path, callBack);
        }

        protected void DoLoadAsync(string path, ResourceAsyncCallBack callback)
        {
            AssetBundleHint hint = HintGet(path);
            if (hint != null)
            {
                HintLoadMainAssetAsync(hint, callback);
            }
            else
            {
                Debug.LogError("没有Asset资源");
                callback(new AsyncResourceRequest());
            }

        }

        protected override void DoLoadAsync<T>(string path, ResourceAsyncCallBack callBack)
        {
            DoLoadAsync(path, callBack);
        }

        protected override void DoUnLoadAsset(Object assetToUnload)
        {
            AssetBundleHint hint;
            int assetInstacneId = assetToUnload.GetInstanceID();
            if (loadedAssetHintMap.TryGetValue(assetInstacneId, out hint))
            {
                if (assetInstacneId != hint.mainAsset.GetInstanceID())
                {
                    Object.Destroy(assetToUnload);
                }
                HintReduceRefCount(hint);
                loadedAssetHintMap.Remove(assetInstacneId);
            }
            else
            {
                Debug.LogError("Asset Not Loaded From Facade:", assetToUnload);
                //Object.Destroy(assetToUnload);                     
            }
        }

#if UNITY_EDITOR

        protected override void DoTrik()
        {
            base.DoTrik();

            DebugLog();
        }

        private void DebugLog()
        {
            var sb = new StringBuilder();
            var bundleList = new List<AssetBundleHint>();
            var assetList = new List<AssetBundleHint>();
            var objList = new List<Object>();
            foreach (var hint in hintMap.Values)
            {
                if (hint.bundle != null)
                {
                    bundleList.Add(hint);
                }

                if (hint.mainAsset != null)
                {
                    assetList.Add(hint);
                }
            }
            sb.AppendLine(string.Format("BundleResourceInfo bundleCount:{0} assetCount:{1} instantiateCOunt:{2}", bundleList.Count, assetList.Count, loadedAssetHintMap.Count));
            sb.AppendLine("\nHintHasBunlde:");
            foreach (var hint in bundleList)
            {
                sb.AppendLine(String.Format("bundlename:{0} \trefCount:{1}", hint.assetName, hint.refCount));
            }
            sb.AppendLine("\nHintHasAsset:");
            foreach (var hint in assetList)
            {
                sb.AppendLine(String.Format("bundlename:{0} \trefCount:{1}", hint.assetName, hint.refCount));
            }

            Debug.Log(sb);
        }
#endif
        #region  Reference Countor

        private Dictionary<string, AssetBundleHint> hintMap;
        private Dictionary<long, AssetBundleHint> loadedAssetHintMap;

        private void HintInit()
        {
            hintMap = new Dictionary<string, AssetBundleHint>();
            loadedAssetHintMap= new Dictionary<long, AssetBundleHint>();
            foreach (var pair in AssetBundleConfig.map)
            {
                var name = pair.Key;
                var node = pair.Value;
                hintMap[name] = new AssetBundleHint()
                {
                    bundlePath = BundleTool.GetBundleFilePath(node.assetName),
                    assetName = node.assetName
                };
            }

            foreach (var pair in AssetBundleConfig.map)
            {
                var name = pair.Key;
                var node = pair.Value;
                var hint = hintMap[name];
                foreach (var depName in node.depenceList)
                {
                    hint.dependenceList.Add(hintMap[depName]);
                }
            }
        }
        private AssetBundleHint HintGet(string path)
        {
            var name = BundleTool.GetAssetName(path);
            AssetBundleHint hint;
            hintMap.TryGetValue(name, out hint);
            return hint;
        }
        private Object HintLoadMainAsset(AssetBundleHint hint, bool instantiate = false)
        {
            if (hint.bundle == null)
            {
                HintLoadAssetBundle(hint);
            }
            if (hint.mainAsset == null)
            {
                hint.mainAsset = hint.bundle.LoadAsset(hint.assetName);
            }
            HintIncreaseRefCount(hint);
            var asset = instantiate ? Object.Instantiate(hint.mainAsset) : hint.mainAsset;
            loadedAssetHintMap.Add(asset.GetInstanceID(), hint);
            return asset;
        }
        private void HintLoadAssetBundle(AssetBundleHint hint)
        {
            foreach (var depHint in hint.dependenceList)
            {
                HintLoadAssetBundle(depHint);
            }
            hint.bundle = hint.bundle ?? AssetBundle.LoadFromFile(hint.bundlePath);
        }
        private void HintRecyle(AssetBundleHint hint)
        {
            hint.mainAsset = null;
            hint.bundle.Unload(true);
            Object.Destroy(hint.bundle);
            hint.bundle = null;
        }
        private void HintReduceRefCount(AssetBundleHint hint)
        {
            foreach (var depHint in hint.dependenceList)
            {
                HintReduceRefCount(depHint);
            }

            hint.refCount--;
            if (hint.refCount == 0)
            {
                HintRecyle(hint);
            }
        }
        private void HintIncreaseRefCount(AssetBundleHint hint)
        {
            foreach (var depHint in hint.dependenceList)
            {
                HintIncreaseRefCount(depHint);
            }

            hint.refCount++;
        }
        #region async
        private void HintLoadAssetBundleAsync(AssetBundleHint hint, ResourceAsyncCallBack callback)
        {
            var mainRequest = new AsyncBundleCreateMainRequest();
            var createRequest = HintCreateBundleAsync(hint);
            mainRequest.callback = callback;
            mainRequest.mainRequest = createRequest;

            AddAsyncCallback(mainRequest, OnAssetBundleLoadComplete);
        }

        private AsyncBundleCreateRequest HintCreateBundleAsync(AssetBundleHint hint)
        {
            var request = AssetBundle.LoadFromFileAsync(hint.bundlePath);

            var bundleRequest = new AsyncBundleCreateRequest();
            bundleRequest.createRequest = request;
            bundleRequest.hint = hint;

            for (int i = 0, count = hint.dependenceList.Count; i < count; i++)
            {
                var depHint = hint.dependenceList[i];
                if (depHint.bundle == null)
                {
                    bundleRequest.dependenceRequestList.Add(HintCreateBundleAsync(depHint));
                }
            }

            return bundleRequest;
        }
        private void OnAssetBundleLoadComplete(IAsyncResourceRequest request)
        {
            var mainCreateRequest = request as AsyncBundleCreateMainRequest;
            DealAsyncBundleCreateRequest(mainCreateRequest.mainRequest);

            DoHintLoadMainAssetAsync(mainCreateRequest.mainRequest.hint, mainCreateRequest.callback);
        }

        private void DealAsyncBundleCreateRequest(AsyncBundleCreateRequest request)
        {
            var createRequest = request;
            var hint = createRequest.hint;
            var assetbundle = createRequest.createRequest.assetBundle;
            if (hint.bundle == null)
            {
                hint.bundle = assetbundle;
            }
            else
            {
                assetbundle.Unload(true);
            }
            int depCount = createRequest.dependenceRequestList.Count;
            for (int i = 0; i < depCount; i++)
            {
                DealAsyncBundleCreateRequest(createRequest.dependenceRequestList[i]);
            }
        }

        private void HintLoadMainAssetAsync(AssetBundleHint hint, ResourceAsyncCallBack callBack)
        {
            //预定
            HintIncreaseRefCount(hint);
            if (hint.bundle == null)
            {
                HintLoadAssetBundleAsync(hint, callBack);
                return;
            }
            DoHintLoadMainAssetAsync(hint, callBack);
        }

        private void DoHintLoadMainAssetAsync(AssetBundleHint hint, ResourceAsyncCallBack callBack)
        {
            if (hint.mainAsset != null)
            {
                HintInvokeCallback(hint, callBack);
                return;
            }
            var request = hint.bundle.LoadAssetAsync(hint.assetName);

            var assetRequest = new AsyncAssetRequest();
            assetRequest.request = request;
            assetRequest.callback = callBack;
            assetRequest.hint = hint;

            AddAsyncCallback(assetRequest, OnAssetLoadComplete);
        }

        private void OnAssetLoadComplete(IAsyncResourceRequest request)
        {
            var assetRequest = request as AsyncAssetRequest;
            var hint = assetRequest.hint;
            var callback = assetRequest.callback;
            var asset = assetRequest.request.asset;
            hint.mainAsset = asset;

            HintInvokeCallback(hint, callback);
        }

        private void HintInvokeCallback(AssetBundleHint hint, ResourceAsyncCallBack callback)
        {
            var resourceRequest = new AsyncResourceRequest();
            var asset = hint.mainAsset;
            loadedAssetHintMap.Add(asset.GetInstanceID(), hint);
            resourceRequest.isDone = true;
            resourceRequest.asset = hint.mainAsset;

            callback(resourceRequest);
        }
        #endregion
        #endregion


    }


}
