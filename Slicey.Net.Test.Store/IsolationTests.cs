using Slicey.Net.Selector;
using Slicey.Net.SliceCloning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slicey.Net.Test.Store
{
    class StoreClass
    {
        public class NestedClass
        {
            public int Int { get; set; }
        }
        public int Int { get; set; }
        public NestedClass NestedClassProp { get; set; } = new();
    }

    class Store : RootStateStore<StoreClass>
    {
        public Store(StoreClass initialState, CloningLevel cloningLevel = CloningLevel.NoCloning) : base(initialState, cloningLevel)
        {
            LowerLevelPropSelector = AddSelector(store => store.NestedClassProp.Int);
            UpperLevelPropSelector = AddSelector(store => store.Int);
            IncreaseLowerLevelProp = AddAction();
            IncreaseUpperLevelProp = AddAction();

            AddReducer(IncreaseLowerLevelProp,
                       store => store.NestedClassProp.Int,
                       store => store.NestedClassProp.Int + 1);
            AddReducer(IncreaseUpperLevelProp,
                     store => store.Int,
                     store => store.Int + 1);
        }

        public Selector<StoreClass, int> LowerLevelPropSelector { get; }
        public Selector<StoreClass, int> UpperLevelPropSelector { get; }
        public StateAction IncreaseLowerLevelProp { get; }
        public StateAction IncreaseUpperLevelProp { get; }
    }

    public class IsolationTests
    {
        [Fact]
        public void Test_DeepCloneIsolation()
        {
            StoreClass initialState = new();
            var store = new Store(initialState, CloningLevel.DeepCloning);
            var selectorHasTheRightValue = new AutoResetEvent(false);
            var expectedValue = 1;
            store.LowerLevelPropSelector.SelectorUpdated += (_, newValue) =>
            {
                if(newValue == expectedValue)
                {
                    selectorHasTheRightValue.Set();
                }
            };
            store.Dispatch(store.IncreaseLowerLevelProp);
            Assert.True(selectorHasTheRightValue.WaitOne(TimeSpan.FromSeconds(2)));
            Assert.Equal(0, initialState.NestedClassProp.Int);

            initialState.NestedClassProp.Int = 100;

            expectedValue = 2;
            store.Dispatch(store.IncreaseLowerLevelProp);
            Assert.True(selectorHasTheRightValue.WaitOne(TimeSpan.FromSeconds(2)));
            Assert.Equal(100, initialState.NestedClassProp.Int);
        }

        [Fact]
        public void Test_ShallowCloneIsolation()
        {
            StoreClass initialState = new();
            var store = new Store(initialState, CloningLevel.ShallowCloning);
            var upperSelectorHasRightValue = new AutoResetEvent(false);
            var lowerSelectorHasRightValue = new AutoResetEvent(false);
            var expectedUpperValue = 1;
            var expectedLowerValue = 2;
            store.UpperLevelPropSelector.SelectorUpdated += (_, newValue) =>
            {
                if (newValue == expectedUpperValue)
                {
                    upperSelectorHasRightValue.Set();
                }
            };

            store.LowerLevelPropSelector.SelectorUpdated += (_, newValue) =>
            {
                if (newValue == expectedLowerValue)
                {
                    lowerSelectorHasRightValue.Set();
                }
            };
            initialState.NestedClassProp.Int = 1;
            store.Dispatch(store.IncreaseLowerLevelProp);
            Assert.True(lowerSelectorHasRightValue.WaitOne(TimeSpan.FromSeconds(2)));
            Assert.Equal(2, initialState.NestedClassProp.Int);

            initialState.Int = 100;
            store.Dispatch(store.IncreaseUpperLevelProp);
            Assert.True(upperSelectorHasRightValue.WaitOne(TimeSpan.FromSeconds(2)));
            Assert.Equal(100, initialState.Int);
        }

        [Fact]
        public void Test_NoCloneIsolation()
        {
            StoreClass initialState = new();
            var store = new Store(initialState, CloningLevel.NoCloning);
            var upperSelectorHasRightValue = new AutoResetEvent(false);
            var lowerSelectorHasRightValue = new AutoResetEvent(false);
            var expectedUpperValue = 101;
            var expectedLowerValue = 2;
            store.UpperLevelPropSelector.SelectorUpdated += (_, newValue) =>
            {
                if (newValue == expectedUpperValue)
                {
                    upperSelectorHasRightValue.Set();
                }
            };

            store.LowerLevelPropSelector.SelectorUpdated += (_, newValue) =>
            {
                if (newValue == expectedLowerValue)
                {
                    lowerSelectorHasRightValue.Set();
                }
            };
            initialState.NestedClassProp.Int = 1;
            store.Dispatch(store.IncreaseLowerLevelProp);
            Assert.True(lowerSelectorHasRightValue.WaitOne(TimeSpan.FromSeconds(2)));
            Assert.Equal(2, initialState.NestedClassProp.Int);

            initialState.Int = 100;
            store.Dispatch(store.IncreaseUpperLevelProp);
            Assert.True(upperSelectorHasRightValue.WaitOne(TimeSpan.FromSeconds(2)));
            Assert.Equal(101, initialState.Int);
        }
    }
}
