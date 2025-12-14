using PipetteGames.Events.Interfaces;

namespace PipetteGames.EventBusExamples
{
    public class DemoEvent : IEvent
    {
        public string Message { get; set; }
        public int Value { get; set; }
    }
}