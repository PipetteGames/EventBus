using UnityEngine;

namespace PipetteGames.EventBusExamples
{
    public class EventPublisher : MonoBehaviour
    {
        public void PublishSimpleEvent()
        {
            EventBusManager.Instance.EventBus.Publish(new DemoEvent { Message = "Simple Event!" });
        }

        public void PublishHighScoreEvent()
        {
            EventBusManager.Instance.EventBus.Publish(new DemoEvent { Message = "High Score!", Value = 150 });
        }

        public async void PublishAsyncEvent()
        {
            Debug.Log("Publishing Async Event...");
            await EventBusManager.Instance.EventBus.PublishAsync(new DemoAsyncEvent { Message = "Async Event!" });
            Debug.Log("Async Event Published");
        }
    }
}
