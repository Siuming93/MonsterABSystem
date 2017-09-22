using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Monster.BaseSystem.ResourceManager
{
    public abstract class BaseResourceManager:IResourceManager
    {
        private List<IAsyncResourceRequest> _doneARList;
        private Dictionary<IAsyncResourceRequest, ResourceAsyncCallBack> _asyncCallBacks;
        
        #region Interface
        public virtual void Init(object data)
        {
            _doneARList = new List<IAsyncResourceRequest>();
            _asyncCallBacks = new Dictionary<IAsyncResourceRequest, ResourceAsyncCallBack>();
        }

        GameObject IResourceManager.LoadPrefab(string path)
        {
            return LoadPrefab(path);
        }

        public Object Load(string path)
        {
            return DoLoad(path, typeof (Object), false);
        }

        public GameObject LoadPrefab(string path)
        {
            return DoLoad(path, typeof(GameObject), true) as GameObject;
        }

        public Object Load(string path, Type systemTypeInstance)
        {
            return DoLoad(path, systemTypeInstance, false) as Object;
        }

        public T Load<T>(string path) where T : Object
        {
            return DoLoad<T>(path);
        }

        public void LoadAsync(string path, ResourceAsyncCallBack callBack)
        {
             DoLoadAsync(path, typeof(Object), callBack);
        }

        public void LoadAsync(string path, Type systemTypeInstance, ResourceAsyncCallBack callBack)
        {
             DoLoadAsync(path, systemTypeInstance, callBack);
        }
        public void LoadAsync<T>(string path, ResourceAsyncCallBack callBack) where T : Object
        {
             DoLoadAsync<T>(path, callBack);
        }

        public void UnLoadAsset(UnityEngine.Object assetToUnload)
        {
            DoUnLoadAsset(assetToUnload);
        }

        public IAsyncResourceRequest UnLoadUnusedAssets()
        {
            return new AsyncOperationRequest(Resources.UnloadUnusedAssets());
        }

        public void Trik()
        {
            DoTrik();
        }
        #endregion

        #region self funcs

        protected virtual void DoTrik()
        {
            _doneARList.Clear();
            var itor = _asyncCallBacks.GetEnumerator();
            while (itor.MoveNext())
            {
                var ar = itor.Current.Key;
                if (ar.isDone)
                {
                    _doneARList.Add(ar);
                }
            }

            for (int i = 0, count = _doneARList.Count; i < count; i++)
            {
                var ar = _doneARList[i];
                var callback = _asyncCallBacks[ar];
                _asyncCallBacks.Remove(ar);
                if (callback != null)
                {
                    callback.Invoke(ar);
                }
            }
        }
        #endregion
        protected void AddAsyncCallback(IAsyncResourceRequest resourceRequest, ResourceAsyncCallBack callback)
        {
            _asyncCallBacks.Add(resourceRequest, callback);
        }

        protected abstract Object DoLoad(string path, Type type, bool instantiate);
        protected abstract void DoLoadAsync(string path, Type type, ResourceAsyncCallBack callBack);


        protected abstract T DoLoad<T>(string path) where T : Object;
        protected abstract void DoLoadAsync<T>(string path, ResourceAsyncCallBack callBack) where T : Object;

        protected abstract void DoUnLoadAsset(Object assetToUnload);
    }
}
