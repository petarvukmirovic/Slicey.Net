using Slicey.Net.Selector;
using Slicey.Net.StateStore;
using System.Linq.Expressions;

namespace Slicey.Net.Test.Store
{
    public class SliceTestStore : SliceStateStore<StoreDataClass, StoreInnerClass>
    {
        public SliceTestStore(RootStateStore<StoreDataClass> rootStore)
               : base(rootStore, x => x.InnerProp)
        {
            AppendSliceString = AddAction<string>("append slice string");
            IncreaseCountStringProp = AddAction("increase record string");

            InnerStringSelector = AddSelector(store => store.StringProp);
            InnerPropAsIntSelector =
                AddSelector(store =>
                                store.RecordProp.StringProp == StoreInnerClass.InitialInnerRecordString ? 
                                0 : int.Parse(store.RecordProp.StringProp)); ;

            RegisterReducers();
        }

        private void RegisterReducers()
        {
            AddReducer(AppendSliceString,
                       store => store.StringProp,
                       (store, arg) => store.StringProp + arg);
            AddReducer(IncreaseCountStringProp,
                       store => store.RecordProp.StringProp,
                       (store) =>                        
                       (int.TryParse(store.RecordProp.StringProp, out var res) ? res + 1 : 1).ToString());
        }

        public StateAction<string> AppendSliceString { get; }
        public StateAction IncreaseCountStringProp { get; }
        public Selector<StoreInnerClass, string> InnerStringSelector { get; }
        public Selector<StoreInnerClass, int> InnerPropAsIntSelector { get; }
    }
}
