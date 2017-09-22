using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Monster.BaseSystem.ResourceManager
{
    public class EditorResourceManager : BaseResourceManager
    {
        private HashSet<Object> _objSet; 

        public override void Init(object data)
        {
            base.Init(data);
            _objSet = new HashSet<Object>();
        }

        protected override Object DoLoad(string path, Type type, bool instantiate)
        {
            var asset =  Resources.Load(path, type);
            var obj = instantiate ? Object.Instantiate(asset) : asset;
            if (instantiate)
            {
                _objSet.Add(obj);
            }
            return obj;
        }


        protected override void DoLoadAsync(string path, Type type, ResourceAsyncCallBack callBack)
        {
            var operation = Resources.LoadAsync(path, type);
            var resourceRequest = new AsyncOperationRequest(operation) {state = callBack};
            AddAsyncCallback(resourceRequest, OnLoadAsyncComplete);
        }

        private void OnLoadAsyncComplete(IAsyncResourceRequest request)
        {
            var operationRequest = request as AsyncOperationRequest;
            var asset = (operationRequest.operation as ResourceRequest).asset;
            var callback = operationRequest.state as ResourceAsyncCallBack;
            callback(new AsyncResourceRequest() {asset = asset});
        }

        protected override T DoLoad<T>(string path)
        {
            return DoLoad(path,typeof(T), false) as T;
        }


        protected override void DoLoadAsync<T>(string path, ResourceAsyncCallBack callBack)
        {
            DoLoadAsync(path, typeof(T), callBack);
        }

        protected override void DoUnLoadAsset(Object assetToUnload)
        {
            if (_objSet.Contains(assetToUnload))
            {
                _objSet.Remove(assetToUnload);
                Object.Destroy(assetToUnload);
                return;
            }
            Resources.UnloadAsset(assetToUnload);
        }
    }
}
