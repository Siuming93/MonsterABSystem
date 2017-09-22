
using UnityEngine;

namespace Monster.BaseSystem.ResourceManager
{
    class AsyncResourceRequest : IAsyncResourceRequest
    {
        public string id { get; set; }
        public bool isDone { get; set; }

        public Object asset;
    }
}
