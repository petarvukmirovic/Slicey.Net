using Slicey.Net.Selector;
using Slicey.Net.StateStore;
using System.Linq.Expressions;

namespace Slicey.Net.Test.BlazorApp.State
{
    public class EchoStateStore : SliceStateStore<AppState, EchoState>
    {
        public EchoStateStore(RootStateStore<AppState> rootStore) : base(rootStore, x => x.EchoState)
        {
            EchoSelector = AddSelector(x => x.Echo);
            UpdateEcho = AddAction<(string user, string message)>();

            AddReducer(UpdateEcho,
                       x => x.Echo,
                       (_, arg) => $"{arg.user} : {arg.message}");
        }

        public Selector<EchoState, string> EchoSelector { get; }
        public StateAction<(string user, string message)> UpdateEcho { get; }
    }
}
