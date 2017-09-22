using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Monster.BaseSystem.ResourceManager
{
    class AsyncBundleCreateMainRequest : IAsyncResourceRequest
    {
        public string id { get; private set; }
        public bool isDone {
            get { return mainRequest.isDone; }
        }

        public AsyncBundleCreateRequest mainRequest;

        public ResourceAsyncCallBack callback;
    }
}
