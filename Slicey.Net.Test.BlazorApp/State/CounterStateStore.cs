using Slicey.Net.Selector;
using Slicey.Net.StateStore;
using System.Linq.Expressions;

namespace Slicey.Net.Test.BlazorApp.State
{
    public class CounterStateStore : SliceStateStore<AppState, CounterState>
    {
        public CounterStateStore(RootStateStore<AppState> rootStore) : base(rootStore, x => x.CounterState)
        {
            CounterSelector = AddSelector(x => x.Counter);
            IncreaseCounter = AddAction();
            AddReducer(IncreaseCounter,
                       x => x.Counter,
                       x => x.Counter + 1);
        }

        public Selector<CounterState, int> CounterSelector { get; }
        public StateAction IncreaseCounter { get; }
    }
}
