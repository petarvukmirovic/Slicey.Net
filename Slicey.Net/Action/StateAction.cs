namespace Slicey.Net
{
    /// <summary>
    /// Represents action that can be dispatched in StateStore
    /// </summary>
    /// <typeparam name="TArg">Type of an argument that is given while dispatching the action</typeparam>
    public class StateAction<TArg>
    {
        internal Guid ActionId;
        public string Description { get; init; } = "";

        internal StateAction(string description = "")
        {
            ActionId = Guid.NewGuid();
            Description = description;
        }

        internal ActionInvocationResult Invoke(TArg arg) =>
            new() { ActionArg = arg!, ActionId = ActionId };
    }

    /// <summary>
    /// Represents action that can be dispatched in StateStore, but does not require an additional arugment
    /// while dispatching.
    /// </summary>

    public class StateAction : StateAction<object?>
    {
        internal StateAction(string description = "") : base(description) { }
    }
}
