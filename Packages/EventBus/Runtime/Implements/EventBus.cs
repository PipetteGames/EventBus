using System;
using System.Collections.Generic;
using System.Linq;

#if EVENTBUS_UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

using PipetteGames.Events.Interfaces;

// TODO: Console.WriteLineはUnityEditor上で読み取れないため、再throwするなどの対応が必要
namespace PipetteGames.Events
{
    public class EventBus : IEventBus
    {
        private readonly Dictionary<Type, object> _handlers = new();
        private readonly Dictionary<Type, object> _cachedHandlers = new();
        private readonly Dictionary<Type, object> _asyncHandlers = new();
        private readonly Dictionary<Type, object> _cachedAsyncHandlers = new();
        private readonly HashSet<Type> _cacheValidTypes = new();
        private readonly HashSet<Type> _asyncCacheValidTypes = new();
        private readonly object _lock = new object();

        private const int DefaultExecutionOrder = 0;

        private class HandlerInfo<T> : IComparable<HandlerInfo<T>> where T : IEvent
        {
            public Action<T> Handler { get; }
            public int ExecutionOrder { get; }
            public Func<T, bool> Filter { get; }

            public HandlerInfo(Action<T> handler, int executionOrder, Func<T, bool> filter)
            {
                Handler = handler;
                ExecutionOrder = executionOrder;
                Filter = filter;
            }

            public int CompareTo(HandlerInfo<T> other)
            {
                // 昇順: 小さい ExecutionOrder が先
                return ExecutionOrder.CompareTo(other.ExecutionOrder);
            }
        }

#if EVENTBUS_UNITASK_SUPPORT
        private class AsyncHandlerInfo<T> where T : IAwaitableEvent
        {
            public Func<T, UniTask> Handler { get; }
            public Func<T, bool> Filter { get; }

            public AsyncHandlerInfo(Func<T, UniTask> handler, Func<T, bool> filter = null)
            {
                Handler = handler;
                Filter = filter;
            }
        }
#else
        private class AsyncHandlerInfo<T> where T : IAwaitableEvent
        {
            public Func<T, Task> Handler { get; }
            public Func<T, bool> Filter { get; }

            public AsyncHandlerInfo(Func<T, Task> handler, Func<T, bool> filter = null)
            {
                Handler = handler;
                Filter = filter;
            }
        }
#endif

        public IEventSubscription Subscribe<T>(Action<T> handler) where T : IEvent
        {
            return Subscribe(handler, DefaultExecutionOrder, null);
        }

        public IEventSubscription Subscribe<T>(Action<T> handler, int executionOrder) where T : IEvent
        {
            return Subscribe(handler, executionOrder, null);
        }

        public IEventSubscription Subscribe<T>(Action<T> handler, Func<T, bool> filter) where T : IEvent
        {
            return Subscribe(handler, DefaultExecutionOrder, filter);
        }

        public IEventSubscription Subscribe<T>(Action<T> handler, int executionOrder, Func<T, bool> filter) where T : IEvent
        {
            lock (_lock)
            {
                var type = typeof(T);
                if (!_handlers.TryGetValue(type, out var obj))
                {
                    obj = new List<HandlerInfo<T>>();
                    _handlers.Add(type, obj);
                }
                var list = (List<HandlerInfo<T>>)obj;
                var info = new HandlerInfo<T>(handler, executionOrder, filter);
                // 二分探索で挿入位置を決め、ソート維持
                int index = list.BinarySearch(info);
                if (index < 0)
                {
                    index = ~index;
                }
                list.Insert(index, info);
                _cacheValidTypes.Remove(type);
                return new EventSubscription(() => Unsubscribe(handler));
            }
        }

        public void Unsubscribe<T>(Action<T> handler) where T : IEvent
        {
            lock (_lock)
            {
                var type = typeof(T);
                if (_handlers.TryGetValue(type, out var obj))
                {
                    var list = (List<HandlerInfo<T>>)obj;
                    list.RemoveAll(info => info.Handler.Equals(handler));
                    if (!list.Any())
                    {
                        _handlers.Remove(type);
                    }
                    _cacheValidTypes.Remove(type);
                }
            }
        }

#if EVENTBUS_UNITASK_SUPPORT
        public IEventSubscription Subscribe<T>(Func<T, UniTask> handler) where T : IAwaitableEvent
        {
            return Subscribe(handler, null);
        }

        public IEventSubscription Subscribe<T>(Func<T, UniTask> handler, Func<T, bool> filter) where T : IAwaitableEvent
        {
            lock (_lock)
            {
                var type = typeof(T);
                if (!_asyncHandlers.TryGetValue(type, out var obj))
                {
                    obj = new List<AsyncHandlerInfo<T>>();
                    _asyncHandlers.Add(type, obj);
                }
                var list = (List<AsyncHandlerInfo<T>>)obj;
                var info = new AsyncHandlerInfo<T>(handler, filter);
                list.Add(info);
                _asyncCacheValidTypes.Remove(type);
                return new EventSubscription(() => Unsubscribe(handler));
            }
        }

        public void Unsubscribe<T>(Func<T, UniTask> handler) where T : IAwaitableEvent
        {
            lock (_lock)
            {
                var type = typeof(T);
                if (_asyncHandlers.TryGetValue(type, out var obj))
                {
                    var list = (List<AsyncHandlerInfo<T>>)obj;
                    list.RemoveAll(info => info.Handler.Equals(handler));
                    if (!list.Any())
                    {
                        _asyncHandlers.Remove(type);
                    }
                    _asyncCacheValidTypes.Remove(type);
                }
            }
        }
#else
        public IEventSubscription Subscribe<T>(Func<T, Task> handler) where T : IAwaitableEvent
        {
            return Subscribe(handler, null);
        }

        public IEventSubscription Subscribe<T>(Func<T, Task> handler, Func<T, bool> filter) where T : IAwaitableEvent
        {
            lock (_lock)
            {
                var type = typeof(T);
                if (!_asyncHandlers.TryGetValue(type, out var obj))
                {
                    obj = new List<AsyncHandlerInfo<T>>();
                    _asyncHandlers.Add(type, obj);
                }
                var list = (List<AsyncHandlerInfo<T>>)obj;
                var info = new AsyncHandlerInfo<T>(handler, filter);
                list.Add(info);
                _asyncCacheValidTypes.Remove(type);
                return new EventSubscription(() => Unsubscribe(handler));
            }
        }

        public void Unsubscribe<T>(Func<T, Task> handler) where T : IAwaitableEvent
        {
            lock (_lock)
            {
                var type = typeof(T);
                if (_asyncHandlers.TryGetValue(type, out var obj))
                {
                    var list = (List<AsyncHandlerInfo<T>>)obj;
                    list.RemoveAll(info => info.Handler.Equals(handler));
                    if (!list.Any())
                    {
                        _asyncHandlers.Remove(type);
                    }
                    _asyncCacheValidTypes.Remove(type);
                }
            }
        }
#endif

        public void Publish<T>(T eventData) where T : IEvent
        {
            List<HandlerInfo<T>> handlersToExecute;
            lock (_lock)
            {
                var type = typeof(T);
                if (!_handlers.TryGetValue(type, out var obj))
                {
                    _cachedHandlers.Remove(type);
                    return;
                }

                if (!_cacheValidTypes.Contains(type))
                {
                    var list = (List<HandlerInfo<T>>)obj;
                    _cachedHandlers[type] = new List<HandlerInfo<T>>(list);
                    _cacheValidTypes.Add(type);
                }
                handlersToExecute = (List<HandlerInfo<T>>)_cachedHandlers[type];
            }

            foreach (var info in handlersToExecute)
            {
                try
                {
                    if (info.Filter == null || info.Filter(eventData))
                    {
                        info.Handler(eventData);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }


#if EVENTBUS_UNITASK_SUPPORT
        public async UniTask PublishAsync<T>(T eventData) where T : IAwaitableEvent
        {
            List<AsyncHandlerInfo<T>> handlersToExecute;
            lock (_lock)
            {
                var type = typeof(T);
                if (!_asyncHandlers.TryGetValue(type, out var obj))
                {
                    _cachedAsyncHandlers.Remove(type);
                    return;
                }

                if (!_asyncCacheValidTypes.Contains(type))
                {
                    var list = (List<AsyncHandlerInfo<T>>)obj;
                    _cachedAsyncHandlers[type] = new List<AsyncHandlerInfo<T>>(list);
                    _asyncCacheValidTypes.Add(type);
                }
                handlersToExecute = (List<AsyncHandlerInfo<T>>)_cachedAsyncHandlers[type];
            }

            var tasks = new List<UniTask>();
            foreach (var info in handlersToExecute)
            {
                try
                {
                    // フィルターチェック
                    if (info.Filter != null && !info.Filter(eventData))
                    {
                        continue;
                    }

                    var task = info.Handler(eventData);
                    tasks.Add(task);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            if (tasks.Count > 0)
            {
                await UniTask.WhenAll(tasks);
            }
        }
#else
        public async Task PublishAsync<T>(T eventData) where T : IAwaitableEvent
        {
            List<AsyncHandlerInfo<T>> handlersToExecute;
            lock (_lock)
            {
                var type = typeof(T);
                if (!_asyncHandlers.TryGetValue(type, out var obj))
                {
                    _cachedAsyncHandlers.Remove(type);
                    return;
                }

                if (!_asyncCacheValidTypes.Contains(type))
                {
                    var list = (List<AsyncHandlerInfo<T>>)obj;
                    _cachedAsyncHandlers[type] = new List<AsyncHandlerInfo<T>>(list);
                    _asyncCacheValidTypes.Add(type);
                }
                handlersToExecute = (List<AsyncHandlerInfo<T>>)_cachedAsyncHandlers[type];
            }

            var tasks = new List<Task>();
            foreach (var info in handlersToExecute)
            {
                try
                {
                    // フィルターチェック
                    if (info.Filter != null && !info.Filter(eventData))
                    {
                        continue;
                    }

                    var task = info.Handler(eventData);
                    if (task != null)
                    {
                        tasks.Add(task);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            if (tasks.Count > 0)
            {
                await Task.WhenAll(tasks);
            }
        }
#endif
    }
}
