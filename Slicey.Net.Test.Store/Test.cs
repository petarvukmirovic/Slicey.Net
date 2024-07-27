using Slicey.Net;
using Slicey.Net.Selector;
using Slicey.Net.SliceCloning;
using Slicey.Net.StateStore;
using System.Linq.Expressions;

//public class AppState
//{
//    public int IntProperty { get; set; }
//    public string StringProperty { get; set; } = "";
//}

//public class AppStateStore : RootStateStore<AppState>
//{
//    public AppStateStore(AppState initialState) : base(initialState)
//    {
//        DoubleIntSelector = AddSelector(state => state.IntProperty * 2);
//        BoldString = AddSelector(state => $"<emph> {state.StringProperty} </emph>");

//        IncreaseInt = AddAction<int>();
//        ResetInt = AddAction();
//        AppendToString = AddAction<string>();

//        AddReducer(IncreaseInt,
//                   state => state.IntProperty,
//                   (state, incAmount) => state.IntProperty + incAmount);
//        AddReducer(ResetInt,
//                   state => state.IntProperty,
//                   (_) => 0);
//        AddReducer(AppendToString,
//                   state => state.StringProperty,
//                   (state, suffix) => state.StringProperty + suffix);
//    }

//    public Selector<AppState, int> DoubleIntSelector { get; }
//    public Selector<AppState, string> BoldString { get; }

//    public StateAction<int> IncreaseInt { get; }
//    public StateAction ResetInt { get; }
//    public StateAction<string> AppendToString { get; }
//}

public class ModuleAState
{
    public string PropA { get; set; } = "";
}

public class ModuleBState
{
    public int PropB { get; set; }
}

public class AppState
{
    public ModuleAState ModuleAState { get; set; } = new();
    public ModuleBState ModuleBState { get; set; } = new();
}

public class AppStateStore : RootStateStore<AppState>
{
    public AppStateStore(AppState initialState) : base(initialState)
    {
        AState = AddSelector(state => state.ModuleAState.PropA);
        BState = AddSelector(state => state.ModuleBState.PropB);
    }

    public Selector<AppState, string> AState { get; }
    public Selector<AppState, int> BState { get; }
}

public class ModuleAStore : SliceStateStore<AppState, ModuleAState>
{
    public ModuleAStore(RootStateStore<AppState> rootStore) : base(rootStore, store => store.ModuleAState)
    {
        // Actions, Selectors and Reducers that are scoped to ModuleAState
    }
}

public class ModuleBStore : SliceStateStore<AppState, ModuleBState>
{
    public ModuleBStore(RootStateStore<AppState> rootStore) : base(rootStore, store => store.ModuleBState)
    {
        // Actions, Selectors and Reducers that are scoped to ModuleBState
    }
}



namespace Slicey.Net.Test.Store
{
    internal class Test
    {
    }
}
