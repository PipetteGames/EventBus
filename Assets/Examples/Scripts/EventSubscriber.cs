using System;
using System.Collections.Generic;
using UnityEngine;

namespace PipetteGames.EventBus.Examples
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