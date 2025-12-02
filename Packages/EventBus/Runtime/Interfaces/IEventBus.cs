using System;

namespace PipetteGames.EventBus
{
    public interface IEventBus
    {
        public ISubscription Subscribe<T>(Action<T> handler) where T : IEvent;
        public ISubscription Subscribe<T>(Action<T> handler, int executionOrder) where T : IEvent;
        public ISubscription Subscribe<T>(Action<T> handler, Func<T, bool> filter) where T : IEvent;
        public ISubscription Subscribe<T>(Action<T> handler, int executionOrder, Func<T, bool> filter) where T : IEvent;
        public void Unsubscribe<T>(Action<T> handler) where T : IEvent;
        public void Publish<T>(T eventData) where T : IEvent;
    }
}
