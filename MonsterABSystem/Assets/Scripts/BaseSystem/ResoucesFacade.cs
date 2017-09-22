#define BUNDLE_MODEL
using Monster.BaseSystem.ResourceManager;

namespace Monster.BaseSystem
{
    public class ResourcesFacade
    {
        private static IResourceManager mInstance;

        public static IResourceManager Instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance =
#if !BUNDLE_MODEL
                    new EditorResourceManager();
#else
                    new BundleResourceManager();
#endif
                }
                return mInstance;
            }
        }
    }
}
