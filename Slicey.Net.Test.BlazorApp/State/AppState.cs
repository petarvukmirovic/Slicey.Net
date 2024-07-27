namespace Slicey.Net.Test.BlazorApp.State
{
    public class CounterState
    {
        public int Counter { get; set; }
    }

    public class EchoState
    {
        public string Echo { get; set; } = string.Empty;
    }

    public class AppState
    {
        public CounterState CounterState { get; } = new();
        public EchoState EchoState { get; set; } = new();
    }
}
