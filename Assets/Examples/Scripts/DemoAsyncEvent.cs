using PipetteGames.Events.Interfaces;

namespace PipetteGames.EventBusExamples
{
    public class DemoAsyncEvent : IAwaitableEvent
    {
        public string Message { get; set; }
        public int Value { get; set; }
    }
}
