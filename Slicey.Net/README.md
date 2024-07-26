# Slicey.Net

## About
An NgRx and Redux inspired library for state management in .NET.

Unlike other .NET Redux libraries, it does not require making classes for all
actions, selectors and reducers, but provides metods that register these objects
in a familiar (almost exactly like in NgRx), intuitive and type-safe way.

Slicey.Net is asynchronous and thread-safe: actions can be dispatched from many
different threads, and they will be processed one by one by a background thread.

Like in NgRx, selector will asynchronously emit new values when they are updated
(using native .NET events)


## Important
The project is in very early development phase. Features that will be added include
* Support for slicing the state in one root store and many slice stores (like NgRx)
* Intelligently updating only the selectors whose state has been updated (by
first preprocessing the expressions that define the selector)
* Allowing actions defined only on slice levels to be processed concurrently.

## Example

Here is an example of a store:

```c#
internal class TestStore : StateStore<StoreDataClass>
{
      public TestStore(StoreDataClass initialState) : base(initialState)
      {
          FlipBool = AddAction("flip bool");
          AppendInnerString = AddAction<string>("append string");
          IncreaseNum = AddAction<int>("incrase num");

          FlippedBoolSelector = AddSelector(store => !store.BoolProp);
          InnerStringSelector = AddSelector(store => store.InnerProp.StringProp);
          DoubleNumSelector = AddSelector(store => (long)(2*store.NumProp));

          RegisterReducers();
      }

      private void RegisterReducers()

      {
          AddReducer(FlipBool, 
                     state => state.BoolProp, 
                     state => !state.BoolProp);
          AddReducer(AppendInnerString, 
                     state => state.InnerProp.StringProp, 
                     (state, suffix) => state.InnerProp.StringProp + suffix);
          AddReducer(IncreaseNum, 
                     state => state.NumProp, 
                     (state, inc) => state.NumProp + inc);
      }

      public StateAction FlipBool { get; }
      public StateAction<string> AppendInnerString { get; }
      public StateAction<int> IncreaseNum { get; }

      public Selector<StoreDataClass, bool> FlippedBoolSelector { get; }
      public Selector<StoreDataClass, string> InnerStringSelector { get; }
      public Selector<StoreDataClass, long> DoubleNumSelector { get; }
}
```
which is dependent on a state data class:
```c#
namespace Slicey.Net.Store.Tests
{
    record class Record(string StringProp);

    internal class StoreInnerClass 
    {
        public int IntProp { get; set; }
        public string StringProp { get; set; } = "";
        public Record RecordProp { get; set; } = new("inner record");
    }

    internal class StoreDataClass
    {
        public int NumProp { get; set; }
        public bool BoolProp { get; set; }
        public string StringReadonlyProp { get; } = "Readonly";
        public string StringWriteableProp { get; set; } = "";
        public StoreInnerClass InnerProp { get; } = new();
    }
}
```

Each selector can be read directly by implicitly converting it to target value type
(for example `(bool)FlippedBoolSelector`) and one can subscribe to changes to values
using `SelectorUpdated` method.
