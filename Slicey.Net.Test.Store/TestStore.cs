using Slicey.Net.Selector;
using Slicey.Net.SliceCloning;
namespace Slicey.Net.Test.Store
{
    internal class TestStore : RootStateStore<StoreDataClass>
    {
        public TestStore(StoreDataClass initialState, CloningLevel level = CloningLevel.DeepCloning) : base(initialState, level)
        {
            FlipBool = AddAction("flip bool");
            AppendInnerString = AddAction<string>("append string");
            IncreaseNum = AddAction<int>("incrase num");
            ResetInnerPropStringTo0 = AddAction("reset 0");

            FlippedBoolSelector = AddSelector(store => !store.BoolProp);
            InnerStringSelector = AddSelector(store => store.InnerProp.StringProp);
            DoubleNumSelector = AddSelector(store => (long)(2*store.NumProp));
            LastCharacterOfPropStringSelector = AddSelector(store => store.InnerProp.RecordProp.StringProp.Last());

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
            AddReducer(ResetInnerPropStringTo0,
                       state => state.InnerProp.RecordProp.StringProp,
                       (_) => "0");
        }

        public StateAction FlipBool { get; }
        public StateAction<string> AppendInnerString { get; }
        public StateAction<int> IncreaseNum { get; }
        public StateAction ResetInnerPropStringTo0 { get; }
        public Selector<StoreDataClass, bool> FlippedBoolSelector { get; }
        public Selector<StoreDataClass, string> InnerStringSelector { get; }
        public Selector<StoreDataClass, long> DoubleNumSelector { get; }
        public Selector<StoreDataClass, char> LastCharacterOfPropStringSelector { get; }
    }
}
