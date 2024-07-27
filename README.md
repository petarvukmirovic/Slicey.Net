# Slicey.Net: strongly-typed Expression-based Redux-like library for .NET

## About
Slicey.Net is an NgRx and Redux inspired library for state management in .NET.

It has been developed with 4 important design principles:
* It takes advantage of native C# concepts like ExpressionTrees for strong typing and\
  events for notifying that state has been updated
* Like NgRx store, it manges state in slices so that store can be separated \
  in multiple easy to manage stores responsible for their own part of the state
* It requires only state stores to inherit from library types, while actions, reducers\
  and selectors are easily added with provided builder methods
* Dependency injection is supported out of the box 

## How to use Slicey.Net

### Implementing the root store
Suppose that your application has to manage state consisting
of the following class:

```c#
class AppState
{
    public int IntProperty { get; set; }
    public string StringProperty { get; set; } = "";
}
```

Suppose further that there are three types of mutations possible:
`IntProperty` can be increased or reset, while `StringProperty`
can only be appended to. Note that we are not interested in the
values of the properties themselves but in double the value of
`IntProperty`and HTML formatted value of `StringProperty`. This
can be achieved with the following store:

```c#
public class AppStateStore : RootStateStore<AppState>
{
    public AppStateStore(AppState initialState) : base(initialState)
    {
        DoubleIntSelector = AddSelector(state => state.IntProperty * 2);
        BoldString = AddSelector(state => $"<emph> {state.StringProperty} </emph>");

        IncreaseInt = AddAction<int>();
        ResetInt = AddAction();
        AppendToString = AddAction<string>();

        AddReducer(IncreaseInt,
                   state => state.IntProperty,
                   (state, incAmount) => state.IntProperty + incAmount);
        AddReducer(ResetInt,
                   state => state.IntProperty,
                   (_) => 0);
        AddReducer(AppendToString,
                   state => state.StringProperty,
                   (state, suffix) => state.StringProperty + suffix);
    }

    public Selector<AppState, int> DoubleIntSelector { get; }
    public Selector<AppState, string> BoldString { get; }
    
    public StateAction<int> IncreaseInt { get; }
    public StateAction ResetInt { get; }
    public StateAction<string> AppendToString { get; }
}
```

* **Selectors** are simply defined using builder method and
  expression that produces a value using the state varibles.
  Their current value can be read by converting the selector
  to the target type of selector (e.g. `(string)BoldString`).
  They also expose event `SelectorUpdated` whenever new value
  is generated for selector.
* **Actions** can have arguments like `IncreaseInt` and `AppendToString`
  or have no arguments like `ResetInt`
* **Reducers** represent rules that describe how the state is updated
  when action occurs. For example, the first reducer defined above states 
  that propery `IntProperty` is incremented by action argument `incAmount`
* Actions are **dispatched** by calling the `Dispatch` method on the store.
  For example, to increase the `IntProperty` by 5 we call `store.Dispatch(store.IncreaseInt, 5)`.
  To reset the same property, we call `store.Dispatch(store.ResetInt)`; 
  This will dispatch an action that will, according to the rules described by reducers
  update the state and invoke the selectors that have been updated.
* Note that selectors and actions are exposed as public properties as they are  
  used outside the store, while reducers are internal rules on how to update
  the state and are not exposed.

### Slicing the state

Now suppose that we have two independent modules that maintain their own state
and a root module that is simply interested in the state of both modules. This can be 
best modelled by `SliceStore`s:

```c#
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
        // Actions, Selectors and Reducers that are scoped to ModuleAState
    }
}
```

### Mutability

Unlike JavaScript based state stores, Slicey.NET does not require that every reducer creates
a new copy of the state. This can lead to unwanted side effects if the objects that are input
(initial state and results of reducer computations) or output (selector values) 
to state store are manipulated outside the store. To mitigate this, Slicey.Net deeply clones 
all the objects that are inputs or outputs to the store.

If this is too expensive for your use case, you override default cloning behavior to either
shallow cloning or no cloning by using the `cloningLevel` argument of the `RootStateStore`.
The same clonning level is used in all `SliceStateStore`s based on this `RootStateStore`.

### Dependency injection

Slicey.Net exposes `RegisterRootStore<TStore, TStoreType>` and `RegisterSliceStore<TStore, TRootType, TStoreType>` 
that can be used to inject the classes in the dependency container. Stores are injected as **singletons**. 

Following the example for slice stores, these would be injected as follows (Slicey.Net supports injecting into 
`IHostBuilder`, `IHostApplicationBuilder` and directly into `IServiceCollection`):

```c#
var builder = WebApplication.CreateBuilder(args);
var initialState = new AppStateStore();
builder.RegisterRootStore<AppStateStore, AppState>(initialState);
builder.RegisterSliceStore<ModuleAStore, AppState, ModuleAState>();
builder.RegisterSliceStore<ModuleBStore, AppState, ModuleBState>();
```