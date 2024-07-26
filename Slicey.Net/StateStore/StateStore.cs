using Slicey.Net.Selector;
using System.Linq.Expressions;

namespace Slicey.Net
{
    public abstract class StateStore<TState>
    {
        protected internal abstract void AddReducer<TSlice, TActionArg>(
            StateAction<TActionArg> actionToReactOn,
            Expression<Func<TState, TSlice>> stateSliceSelector,
            Func<TState, TActionArg, TSlice> updater);
        protected internal abstract void AddReducer<TSlice>(
            StateAction actionToReactOn,
            Expression<Func<TState, TSlice>> stateSlice,
            Func<TState, TSlice> updater);
        protected internal abstract Selector<TState, TSlice> AddSelector<TSlice>(Expression<Func<TState, TSlice>> selectorExpression);
        protected StateAction<TActionArg> AddAction<TActionArg>(string description = "") => new(description);
        protected StateAction AddAction(string description = "") => new(description);
        public abstract void Dispatch<TActionArg>(StateAction<TActionArg> action, TActionArg argument);
        public abstract void Dispatch(StateAction action);
    }
}