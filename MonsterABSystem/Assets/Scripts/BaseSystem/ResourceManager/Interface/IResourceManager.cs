using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Monster.BaseSystem.ResourceManager
{
    public interface IResourceManager
    {
        void Init(object data);

        void Trik();

        /// <summary>
        /// 加载prefab
        /// </summary>
        /// <param name="path"></param>
        /// <returns>已经实例化的GameObject</returns>
        GameObject LoadPrefab(string path);
        Object Load(string path);
        Object Load(string path, Type type);
        T Load<T>(string path) where T : Object;
        void LoadAsync(string path, ResourceAsyncCallBack callBack);
        void LoadAsync<T>(string path, ResourceAsyncCallBack callBack) where T : Object;
        void LoadAsync(string path, Type systemTypeInstance, ResourceAsyncCallBack callBack);
        void UnLoadAsset(Object assetToUnload);
        IAsyncResourceRequest UnLoadUnusedAssets();

      
    }
}
