namespace Monster.BaseSystem.ResourceManager
{
    public interface IAsyncResourceRequest
    {
        string id { get; }
        bool isDone { get; }
    }
}
