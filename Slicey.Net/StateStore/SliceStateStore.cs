using Slicey.Net.ExpressionExtensions;
using Slicey.Net.Selector;
using System.Linq.Expressions;

namespace Slicey.Net.StateStore
{
    public class SliceStateStore<TRootState, TSlice> : StateStore<TSlice>
    {
        private readonly RootStateStore<TRootState> rootStore;
        private readonly Expression<Func<TRootState, TSlice>> sliceSelector;
        private readonly Func<TRootState, TSlice> sliceSelectorCompiled;

        public SliceStateStore(RootStateStore<TRootState> rootStore, Expression<Func<TRootState, TSlice>> sliceSelector)
        {
            if(!sliceSelector.IsPropertyAccessorChain(requireWriteable:false))
            {
                throw new ArgumentException("slice selector must be property accessor chain (e.g. x => x.A.B)");
            }

            this.rootStore = rootStore;
            this.sliceSelector = sliceSelector;
            sliceSelectorCompiled = sliceSelector.Compile();
        }

        protected internal override void AddReducer<TSlicesSlice, TActionArg>(
            StateAction<TActionArg> actionToReactOn, 
            Expression<Func<TSlice, TSlicesSlice>> stateSliceSelector, 
            Func<TSlice, TActionArg, TSlicesSlice> updater) =>
            rootStore.AddReducer(actionToReactOn, 
                                 sliceSelector.ConcatenateProperyAccessors(stateSliceSelector), 
                                 (root, arg) => updater(sliceSelectorCompiled(root), arg));

        protected internal override void AddReducer<TSlicesSlice>(
            StateAction actionToReactOn, 
            Expression<Func<TSlice, TSlicesSlice>> stateSlice, 
            Func<TSlice, TSlicesSlice> updater) =>
            rootStore.AddReducer(actionToReactOn,
                                 sliceSelector.ConcatenateProperyAccessors(stateSlice),
                                 (root) => updater(sliceSelectorCompiled(root)));

        protected internal override Selector<TSlice, TSlicesSlice> AddSelector<TSlicesSlice>(Expression<Func<TSlice, TSlicesSlice>> selectorExpression) =>
            rootStore.AddSliceSelector(sliceSelector, selectorExpression);

        public override void Dispatch<TActionArg>(StateAction<TActionArg> action, TActionArg argument) => rootStore.Dispatch(action, argument);
        public override void Dispatch(StateAction action) => rootStore.Dispatch(action);
    }
}
