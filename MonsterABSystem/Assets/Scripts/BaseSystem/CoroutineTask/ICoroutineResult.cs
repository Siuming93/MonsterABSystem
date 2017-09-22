using System.Collections;

namespace Monster.BaseSystem.CoroutineTask
{
    interface ICoroutineResult
    {
        /// <summary>
        /// 指示异步操作是否已完成
        /// </summary>
        bool IsCompleted { get; }
        /// <summary>
        /// 获取一个值,
        /// </summary>
        //IEnumerator CoroutineEnumerator { get; }
        /// <summary>
        /// 用户定义的对象
        /// </summary>
        object AsyncState { get; }
    }
}
