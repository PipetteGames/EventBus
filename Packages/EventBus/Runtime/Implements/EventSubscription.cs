using System;
using PipetteGames.Events.Interfaces;

namespace PipetteGames.Events
{
    public class EventSubscription : IEventSubscription
    {
        private readonly Action _unsubscribe;

        public EventSubscription(Action unsubscribe)
        {
            _unsubscribe = unsubscribe;
        }

        public void Dispose()
        {
            _unsubscribe?.Invoke();
        }
    }
}
