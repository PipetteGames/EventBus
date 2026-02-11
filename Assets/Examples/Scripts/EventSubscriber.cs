using System;
using System.Collections.Generic;

#if EVENTBUS_UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

using UnityEngine;

namespace PipetteGames.EventBusExamples
{
    public class EventSubscriber : MonoBehaviour
    {
        private List<IDisposable> _subscriptions = new();

        private void Start()
        {
            var eventBus = EventBusManager.Instance.EventBus;

            // 基本購読
            _subscriptions.Add(eventBus.Subscribe<DemoEvent>(OnDemoEvent));

            // 実行順指定
            _subscriptions.Add(eventBus.Subscribe<DemoEvent>(OnOrderedEvent, executionOrder: 5));

            // 実行条件指定
            _subscriptions.Add(eventBus.Subscribe<DemoEvent>(OnFilteredEvent, filter: e => e.Value > 100));

            // 実行順指定 + 実行条件指定
            _subscriptions.Add(eventBus.Subscribe<DemoEvent>(OnOrderedAndFilteredEvent, executionOrder: 10, filter: e => e.Value > 100));

            // 非同期イベント購読
            _subscriptions.Add(eventBus.Subscribe<DemoAsyncEvent>(OnAsyncDemoEvent));

            // 非同期イベント購読 (実行順指定)
            _subscriptions.Add(eventBus.Subscribe<DemoAsyncEvent>(OnAsyncOrderedEvent, executionOrder: 5));
        }

        private void OnDemoEvent(DemoEvent e)
        {
            Debug.Log($"Simple Received: {e.Message} (Value: {e.Value})");
        }

        private void OnOrderedEvent(DemoEvent e)
        {
            Debug.Log($"Ordered Received: {e.Message} (Executed later)");
        }

        private void OnFilteredEvent(DemoEvent e)
        {
            Debug.Log($"Filtered Received: {e.Message} (Value: {e.Value})");
        }

        private void OnOrderedAndFilteredEvent(DemoEvent e)
        {
            Debug.Log($"Ordered and Filtered Received: {e.Message} (Value: {e.Value}) (Executed later)");
        }

#if EVENTBUS_UNITASK_SUPPORT
        private async UniTask OnAsyncDemoEvent(DemoAsyncEvent e)
        {
            Debug.Log($"Async Received: {e.Message} (Value: {e.Value})");
            // 非同期処理の例
            await UniTask.Delay(500);
            Debug.Log($"Async Completed: {e.Message}");
        }

        private async UniTask OnAsyncOrderedEvent(DemoAsyncEvent e)
        {
            Debug.Log($"Async Ordered Received: {e.Message} (Executed later)");
            // 非同期処理の例
            await UniTask.Delay(300);
            Debug.Log($"Async Ordered Completed: {e.Message}");
        }
#else
        private async Task OnAsyncDemoEvent(DemoAsyncEvent e)
        {
            Debug.Log($"Async Received: {e.Message} (Value: {e.Value})");
            // 非同期処理の例
            await Task.Delay(500);
            Debug.Log($"Async Completed: {e.Message}");
        }

        private async Task OnAsyncOrderedEvent(DemoAsyncEvent e)
        {
            Debug.Log($"Async Ordered Received: {e.Message} (Executed later)");
            // 非同期処理の例
            await Task.Delay(300);
            Debug.Log($"Async Ordered Completed: {e.Message}");
        }
#endif

        private void OnDestroy()
        {
            // 購読解除
            foreach (var subscription in _subscriptions)
            {
                subscription.Dispose();
            }
        }
    }
}