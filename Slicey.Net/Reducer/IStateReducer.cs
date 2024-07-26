namespace Slicey.Net
{
    internal interface IStateReducer
    {
        internal bool UpdateStateOnMatchingAction(Guid actionId, dynamic actionArg);
    }
}