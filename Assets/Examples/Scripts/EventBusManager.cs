using PipetteGames.Events;
using PipetteGames.Events.Interfaces;
using UnityEngine;

namespace PipetteGames.EventBusExamples
{
    public class EventBusManager : MonoBehaviour
    {
        public static EventBusManager Instance { get; private set; }
        public IEventBus EventBus { get; private set; } = new EventBus();

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}