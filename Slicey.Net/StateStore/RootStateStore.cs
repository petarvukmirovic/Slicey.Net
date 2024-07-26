using Slicey.Net.ExpressionExtensions;
using Slicey.Net.Selector;
using Slicey.Net.SliceCloning;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Slicey.Net
{
    /// <summary>
    /// Class that implements the Redux-like store. Allows you to register StateActions, Reducers and 
    /// Selectors which will be used in the following way: StateStore dispatches StateAction by enqueueing it
    /// for processing. Then, StateStore will process the enqueued action in a background thread, 
    /// invoking all the reducers that are waiting for the action and updating all the selectors.
    /// </summary>
    /// <typeparam name="TState">Type of the data that is maintained in the store</typeparam>
    public abstract class RootStateStore<TState> : StateStore<TState>, IDisposable
    {
        private TState State;
        private readonly ICloner cloner;
        protected RootStateStore(TState initialState, CloningLevel cloningLevel = CloningLevel.NoCloning) 
        {
            cloner = cloningLevel.ToCloner();
            State = cloner.Clone(initialState);
            UpdateTask = Task.Run(UpdateStateOnAction);
        }
        internal TState CurrentState => State;
        private readonly List<IStateReducer> AllReducers = [];
        private readonly List<IUpdateableSelector<TState>> AllSelectors = [];
        private readonly BlockingCollection<ActionInvocationResult> ActionInvocationQueue = [];
        private readonly CancellationTokenSource CancellationToken = new();
        private readonly Task UpdateTask;

        internal void UpdateState<TStateSlice>(Expression<Func<TState, TStateSlice>> stateSliceSelector, TStateSlice newValue) =>
            stateSliceSelector.UpdateReference(ref State, cloner.Clone(newValue));
        
        /// <summary>
        /// Registers a reducer that will update the state on an action that expectes an argument
        /// </summary>
        /// <param name="actionToReactOn">Action on which to react</param>
        /// <param name="stateSliceSelector">Expression that selects a property within state (or even the whole state) 
        ///                                 that will be updated in-place.</param>
        /// <param name="updater">Function that computes the new value of state (property)</param>
        protected internal override void AddReducer<TSlice, TActionArg>(
            StateAction<TActionArg> actionToReactOn,
            Expression<Func<TState, TSlice>> stateSliceSelector,
            Func<TState, TActionArg, TSlice> updater)
        {
            var reducer = new Reducer<TState, TSlice, TActionArg>(this, actionToReactOn, stateSliceSelector, updater);
            AllReducers.Add(reducer);
        }

        /// <summary>
        /// Registers a reducer that will update the state on an action that expects no argument
        /// </summary>
        /// <param name="actionToReactOn">Action on which to react</param>
        /// <param name="stateSliceSelector">Expression that selects a property within state (or even the whole state) 
        ///                                 that will be updated in-place.</param>
        /// <param name="updater">Function that computes the new value of state (property)</param>
        protected internal override void AddReducer<TSlice>(
            StateAction actionToReactOn,
            Expression<Func<TState, TSlice>> stateSlice,
            Func<TState, TSlice> updater)
        {
            var reducer = new Reducer<TState, TSlice>(this, actionToReactOn, stateSlice, updater);
            AllReducers.Add(reducer);
        }

        /// <summary>
        /// Registers new selector that will be updated whenever state changes.
        /// </summary>
        /// <param name="selectorExpression"></param>
        /// <returns></returns>
        protected internal override Selector<TState, TResult> AddSelector<TResult>(Expression<Func<TState, TResult>> selectorExpression)
        {
            var selector = new Selector<TState, TResult>(CurrentState, selectorExpression, cloner);
            AllSelectors.Add(selector);
            return selector;
        }

        internal Selector<TSlice, TResult> AddSliceSelector<TSlice, TResult>(Expression<Func<TState, TSlice>> sliceSelectorExpression,
                                                                                      Expression<Func<TSlice, TResult>> selectorExpression)
        {
            var sliceSelector = new SliceSelector<TState, TSlice, TResult>(CurrentState, sliceSelectorExpression, selectorExpression, cloner);
            AllSelectors.Add(sliceSelector);
            return sliceSelector;
        }


        /// <summary>
        /// Dispatches action for asynchronous processing. The state will not be immediately updated,
        /// but upon successful processing, all updated selectors will emit new values.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="argument"></param>
        public override void Dispatch<TActionArg>(StateAction<TActionArg> action, TActionArg argument)
        {
            var actionInvocation = action.Invoke(argument);
            ActionInvocationQueue.Add(actionInvocation);
        }

        public override void Dispatch(StateAction action) => Dispatch(action, null);

        private void UpdateStateOnAction()
        {
            while(!CancellationToken.IsCancellationRequested)
            {
                var actionInvocation = ActionInvocationQueue.Take(CancellationToken.Token);
                if (!CancellationToken.IsCancellationRequested)
                { 
                    InvokeAllReducers(actionInvocation);
                    UpdateAllSelectors();
                }
            }
        }

        private void UpdateAllSelectors()
        {
            foreach(var selector in  AllSelectors)
            {
                selector.UpdateSelectorResult(CurrentState);
            }
        }

        private void InvokeAllReducers(ActionInvocationResult actionInvocation)
        {
            foreach(var reducer in AllReducers)
            {
                reducer.UpdateStateOnMatchingAction(actionInvocation.ActionId, actionInvocation.ActionArg);
            }
        }

        public void Dispose()
        {
            CancellationToken.Cancel();
            ActionInvocationQueue.Dispose();
            UpdateTask.Dispose();
        }
    }
}
