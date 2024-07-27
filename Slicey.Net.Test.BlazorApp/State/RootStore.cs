using Slicey.Net.Selector;
using Slicey.Net.SliceCloning;

namespace Slicey.Net.Test.BlazorApp.State
{
    public class RootStore : RootStateStore<AppState>
    {
        public RootStore(AppState initialState) : base(initialState, cloningLevel:CloningLevel.NoCloning)
        {
            CounterValueSelector = AddSelector(x => x.CounterState.Counter);
            EchoValueSelector = AddSelector(x => x.EchoState.Echo);
        }

        public Selector<AppState, int> CounterValueSelector { get; }
        public Selector<AppState, string> EchoValueSelector { get; }
    }
}
