using System;

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
        /// <summary>
        /// イベントの購読を解除する
        /// </summary>
        /// <typeparam name="T">購読を解除するイベントの種類</typeparam>
        /// <param name="handler">イベントハンドラー</param>
        public void Unsubscribe<T>(Action<T> handler) where T : IEvent;
        /// <summary>
        /// イベントを発行する
        /// </summary>
        /// <typeparam name="T">発行するイベントの種類</typeparam>
        /// <param name="eventData">イベントデータ</param>
        public void Publish<T>(T eventData) where T : IEvent;
    }
}
