using System;
using PipetteGames.Events.Interfaces;

namespace PipetteGames.Events
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
