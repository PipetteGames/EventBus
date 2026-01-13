using System;
using PipetteGames.Events.Interfaces;

namespace PipetteGames.Events
{
    internal class EventSubscription : IEventSubscription
    {
        private Action _unsubscribe;
        private bool _isDisposed;

        public EventSubscription(Action unsubscribe)
        {
            _unsubscribe = unsubscribe ?? throw new ArgumentNullException(nameof(unsubscribe));
            _isDisposed = false;
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _unsubscribe?.Invoke();
            _unsubscribe = null;
            _isDisposed = true;
        }
    }
}
