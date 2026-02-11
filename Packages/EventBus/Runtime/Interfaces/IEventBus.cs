using System;

#if EVENTBUS_UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace PipetteGames.Events.Interfaces
{
    /// <summary>
    /// イベントバスを表すインターフェース
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// イベントを購読する
        /// </summary>
        /// <typeparam name="T">購読するイベントの種類</typeparam>
        /// <param name="handler">イベントハンドラー</param>
        /// <returns>購読追跡インスタンス</returns>
        public IEventSubscription Subscribe<T>(Action<T> handler) where T : IEvent;
        /// <summary>
        /// イベントを購読する (実行順序指定付き)
        /// </summary>
        /// <typeparam name="T">購読するイベントの種類</typeparam>
        /// <param name="handler">イベントハンドラー</param>
        /// <param name="executionOrder">実行順序</param>
        /// <returns>購読追跡インスタンス</returns>
        public IEventSubscription Subscribe<T>(Action<T> handler, int executionOrder) where T : IEvent;
        /// <summary>
        /// イベントを購読する (フィルター付き)
        /// </summary>
        /// <typeparam name="T">購読するイベントの種類</typeparam>
        /// <param name="handler">イベントハンドラー</param>
        /// <param name="filter">フィルター関数</param>
        /// <returns>購読追跡インスタンス</returns>
        public IEventSubscription Subscribe<T>(Action<T> handler, Func<T, bool> filter) where T : IEvent;
        /// <summary>
        /// イベントを購読する (実行順序およびフィルター付き)
        /// </summary>
        /// <typeparam name="T">購読するイベントの種類</typeparam>
        /// <param name="handler">イベントハンドラー</param>
        /// <param name="executionOrder">実行順序</param>
        /// <param name="filter">フィルター関数</param>
        /// <returns>購読追跡インスタンス</returns>
        public IEventSubscription Subscribe<T>(Action<T> handler, int executionOrder, Func<T, bool> filter) where T : IEvent;
        
#if EVENTBUS_UNITASK_SUPPORT
        /// <summary>
        /// 非同期イベントを購読する
        /// </summary>
        /// <typeparam name="T">購読するイベントの種類</typeparam>
        /// <param name="handler">非同期イベントハンドラー (UniTask型)</param>
        /// <returns>購読追跡インスタンス</returns>
        public IEventSubscription Subscribe<T>(Func<T, UniTask> handler) where T : IAwaitableEvent;
        /// <summary>
        /// 非同期イベントを購読する (フィルター付き)
        /// </summary>
        /// <typeparam name="T">購読するイベントの種類</typeparam>
        /// <param name="handler">非同期イベントハンドラー (UniTask型)</param>
        /// <param name="filter">フィルター関数</param>
        /// <returns>購読追跡インスタンス</returns>
        public IEventSubscription Subscribe<T>(Func<T, UniTask> handler, Func<T, bool> filter) where T : IAwaitableEvent;
#else
        /// <summary>
        /// 非同期イベントを購読する
        /// </summary>
        /// <typeparam name="T">購読するイベントの種類</typeparam>
        /// <param name="handler">非同期イベントハンドラー (Task型)</param>
        /// <returns>購読追跡インスタンス</returns>
        public IEventSubscription Subscribe<T>(Func<T, Task> handler) where T : IAwaitableEvent;
        /// <summary>
        /// 非同期イベントを購読する (フィルター付き)
        /// </summary>
        /// <typeparam name="T">購読するイベントの種類</typeparam>
        /// <param name="handler">非同期イベントハンドラー (Task型)</param>
        /// <param name="filter">フィルター関数</param>
        /// <returns>購読追跡インスタンス</returns>
        public IEventSubscription Subscribe<T>(Func<T, Task> handler, Func<T, bool> filter) where T : IAwaitableEvent;
#endif
        
        /// <summary>
        /// イベントの購読を解除する
        /// </summary>
        /// <typeparam name="T">購読を解除するイベントの種類</typeparam>
        /// <param name="handler">イベントハンドラー</param>
        public void Unsubscribe<T>(Action<T> handler) where T : IEvent;
        
#if EVENTBUS_UNITASK_SUPPORT
        /// <summary>
        /// 非同期イベントの購読を解除する
        /// </summary>
        /// <typeparam name="T">購読を解除するイベントの種類</typeparam>
        /// <param name="handler">非同期イベントハンドラー (UniTask型)</param>
        public void Unsubscribe<T>(Func<T, UniTask> handler) where T : IAwaitableEvent;
#else
        /// <summary>
        /// 非同期イベントの購読を解除する
        /// </summary>
        /// <typeparam name="T">購読を解除するイベントの種類</typeparam>
        /// <param name="handler">非同期イベントハンドラー (Task型)</param>
        public void Unsubscribe<T>(Func<T, Task> handler) where T : IAwaitableEvent;
#endif
        
        /// <summary>
        /// イベントを発行する
        /// </summary>
        /// <typeparam name="T">発行するイベントの種類</typeparam>
        /// <param name="eventData">イベントデータ</param>
        public void Publish<T>(T eventData) where T : IEvent;
        
#if EVENTBUS_UNITASK_SUPPORT
        /// <summary>
        /// 非同期イベントを発行し、すべてのハンドラーの完了を待つ
        /// </summary>
        /// <typeparam name="T">発行するイベントの種類</typeparam>
        /// <param name="eventData">イベントデータ</param>
        public UniTask PublishAsync<T>(T eventData) where T : IAwaitableEvent;
#else
        /// <summary>
        /// 非同期イベントを発行し、すべてのハンドラーの完了を待つ
        /// </summary>
        /// <typeparam name="T">発行するイベントの種類</typeparam>
        /// <param name="eventData">イベントデータ</param>
        public Task PublishAsync<T>(T eventData) where T : IAwaitableEvent;
#endif
    }
}
