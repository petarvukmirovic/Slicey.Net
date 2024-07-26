using System.Linq.Expressions;

namespace Slicey.Net
{
    internal class Reducer<TState, TStateSlice, TActionArg> : IStateReducer
    {
        internal Reducer(RootStateStore<TState> store, 
                       StateAction<TActionArg> action, 
                       Expression<Func<TState, TStateSlice>> stateSlice, 
                       Func<TState, TActionArg, TStateSlice> updater)
        {
            parentStore = store;
            ActionToReactOn = action;
            StateSliceSelector = stateSlice;
            StateSliceUpdater = updater;
        }

        private readonly RootStateStore<TState> parentStore;
        private StateAction<TActionArg> ActionToReactOn { get; init; }
        private Expression<Func<TState, TStateSlice>> StateSliceSelector { get; init; }
        private Func<TState, TActionArg, TStateSlice> StateSliceUpdater { get; init; }

        internal void UpdateState(TActionArg newArgValue)  =>
            parentStore.UpdateState(StateSliceSelector, StateSliceUpdater(parentStore.CurrentState, newArgValue));

        bool IStateReducer.UpdateStateOnMatchingAction(Guid actionId, dynamic actionArg)
        {
            if (ActionToReactOn.ActionId == actionId)
            {
                TActionArg actionArgTyped = actionArg;
                UpdateState(actionArgTyped);
                return true;
            }
            return false;
        }
    }

    internal class Reducer<TState, TStateSlice> : Reducer<TState, TStateSlice, object?>
    {
        internal Reducer(RootStateStore<TState> store, StateAction action, Expression<Func<TState, TStateSlice>> stateSlice, Func<TState, TStateSlice> updater) 
            : base(store, action, stateSlice, (state, _) => updater(state))
        {
        }
    }
}
