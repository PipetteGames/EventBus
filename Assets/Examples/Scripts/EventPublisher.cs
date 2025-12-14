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
    }
}