using System;
using System.Collections.Generic;
using System.Linq;

#if UNITASK_EVENTBUS_SUPPORT
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

using PipetteGames.Events.Interfaces;

namespace PipetteGames.Events
{
    public class EventBus : IEventBus
    {
        private readonly Dictionary<Type, object> _handlers = new();
        private readonly Dictionary<Type, object> _cachedHandlers = new();
        private readonly Dictionary<Type, object> _asyncHandlers = new();
        private readonly Dictionary<Type, object> _cachedAsyncHandlers = new();
        private readonly object _lock = new object();
        private bool _isCacheValid = false;
        private bool _isAsyncCacheValid = false;

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

#if UNITASK_EVENTBUS_SUPPORT
        private class AsyncHandlerInfo<T> : IComparable<AsyncHandlerInfo<T>> where T : IAwaitableEvent
        {
            public Func<T, UniTask> Handler { get; }
            public int ExecutionOrder { get; }

            public AsyncHandlerInfo(Func<T, UniTask> handler, int executionOrder)
            {
                Handler = handler;
                ExecutionOrder = executionOrder;
            }

            public int CompareTo(AsyncHandlerInfo<T> other)
            {
                // 昇順: 小さい ExecutionOrder が先
                return ExecutionOrder.CompareTo(other.ExecutionOrder);
            }
        }
#else
        private class AsyncHandlerInfo<T> : IComparable<AsyncHandlerInfo<T>> where T : IAwaitableEvent
        {
            public Func<T, Task> Handler { get; }
            public int ExecutionOrder { get; }

            public AsyncHandlerInfo(Func<T, Task> handler, int executionOrder)
            {
                Handler = handler;
                ExecutionOrder = executionOrder;
            }

            public int CompareTo(AsyncHandlerInfo<T> other)
            {
                // 昇順: 小さい ExecutionOrder が先
                return ExecutionOrder.CompareTo(other.ExecutionOrder);
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
                _isCacheValid = false;
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
                    _isCacheValid = false;
                }
            }
        }

#if UNITASK_EVENTBUS_SUPPORT
        public IEventSubscription Subscribe<T>(Func<T, UniTask> handler) where T : IAwaitableEvent
        {
            return Subscribe(handler, DefaultExecutionOrder);
        }

        public IEventSubscription Subscribe<T>(Func<T, UniTask> handler, int executionOrder) where T : IAwaitableEvent
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
                var info = new AsyncHandlerInfo<T>(handler, executionOrder);
                // 二分探索で挿入位置を決め、ソート維持
                int index = list.BinarySearch(info);
                if (index < 0)
                {
                    index = ~index;
                }
                list.Insert(index, info);
                _isAsyncCacheValid = false;
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
                    _isAsyncCacheValid = false;
                }
            }
        }
#else
        public IEventSubscription Subscribe<T>(Func<T, Task> handler) where T : IAwaitableEvent
        {
            return Subscribe(handler, DefaultExecutionOrder);
        }

        public IEventSubscription Subscribe<T>(Func<T, Task> handler, int executionOrder) where T : IAwaitableEvent
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
                var info = new AsyncHandlerInfo<T>(handler, executionOrder);
                // 二分探索で挿入位置を決め、ソート維持
                int index = list.BinarySearch(info);
                if (index < 0)
                {
                    index = ~index;
                }
                list.Insert(index, info);
                _isAsyncCacheValid = false;
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
                    _isAsyncCacheValid = false;
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

                if (!_isCacheValid)
                {
                    var list = (List<HandlerInfo<T>>)obj;
                    _cachedHandlers[type] = new List<HandlerInfo<T>>(list);
                    _isCacheValid = true;
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


#if UNITASK_EVENTBUS_SUPPORT
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

                if (!_isAsyncCacheValid)
                {
                    var list = (List<AsyncHandlerInfo<T>>)obj;
                    _cachedAsyncHandlers[type] = new List<AsyncHandlerInfo<T>>(list);
                    _isAsyncCacheValid = true;
                }
                handlersToExecute = (List<AsyncHandlerInfo<T>>)_cachedAsyncHandlers[type];
            }

            var tasks = new List<UniTask>();
            foreach (var info in handlersToExecute)
            {
                try
                {
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

                if (!_isAsyncCacheValid)
                {
                    var list = (List<AsyncHandlerInfo<T>>)obj;
                    _cachedAsyncHandlers[type] = new List<AsyncHandlerInfo<T>>(list);
                    _isAsyncCacheValid = true;
                }
                handlersToExecute = (List<AsyncHandlerInfo<T>>)_cachedAsyncHandlers[type];
            }

            var tasks = new List<Task>();
            foreach (var info in handlersToExecute)
            {
                try
                {
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
