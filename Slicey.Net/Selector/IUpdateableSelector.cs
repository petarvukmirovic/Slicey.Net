namespace Slicey.Net.Selector
{
    internal interface IUpdateableSelector<TState>
    {
        internal void UpdateSelectorResult(TState newState);
    }
}
