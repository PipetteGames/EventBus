using System;

namespace PipetteGames.EventBus
{
    public class Subscription : ISubscription
    {
        private readonly Action _unsubscribe;

        public Subscription(Action unsubscribe)
        {
            _unsubscribe = unsubscribe;
        }

        public void Dispose()
        {
            _unsubscribe?.Invoke();
        }
    }
}
