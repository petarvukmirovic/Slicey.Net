namespace Slicey.Net
{
    internal class ActionInvocationResult
    {
        public required object ActionArg { get; init; }
        internal Guid ActionId { get; init; }
    }
}
