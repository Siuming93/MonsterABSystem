using UnityEngine;

namespace Monster.BaseSystem.ResourceManager
{
    class AsyncAssetRequest: IAsyncResourceRequest
    {
        public string id { get; set; }
        public bool isDone { get { return request.isDone; } }

        public AssetBundleRequest request;

        public AssetBundleHint hint;

        public ResourceAsyncCallBack callback;

    }
}
