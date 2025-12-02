namespace PipetteGames.EventBus.Examples
{
    public class DemoEvent : IEvent
    {
        public string Message { get; set; }
        public int Value { get; set; }
    }
}