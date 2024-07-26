using Xunit.Abstractions;
using Xunit.Repeat;

namespace Slicey.Net.Test.Store
{
    public class StoreTests
    {
        private TestStore testStore = new(new());
        private readonly SliceTestStore sliceTestStore;
        private readonly ITestOutputHelper output;

        public StoreTests(ITestOutputHelper output)
        {
            var x = new StoreDataClass();
            this.output = output;
            sliceTestStore = new SliceTestStore(testStore);
        }

        [Fact]
        public void Test_SingleThread()
        {
            var selectorHasTheRightValue = new AutoResetEvent(false);
            testStore.InnerStringSelector.SelectorUpdated += (_, newState) =>
            {
                if(newState == "a_b_c")
                {
                    selectorHasTheRightValue.Set();
                }
            };
            testStore.Dispatch(testStore.AppendInnerString, "a_");
            testStore.Dispatch(testStore.AppendInnerString, "b_");
            testStore.Dispatch(testStore.AppendInnerString, "c");
            Assert.True(selectorHasTheRightValue.WaitOne(millisecondsTimeout: 2000));
        }

        [Fact]
        public void Test_Slice_StringProp()
        {
            var innerSelectorUpdated = new AutoResetEvent(false);
            var outerSelectorUpdated = new AutoResetEvent(false);
            var expectedState = "a";

            sliceTestStore.InnerStringSelector.SelectorUpdated += (_, newState) =>
            {
                if (newState == expectedState)
                {
                    innerSelectorUpdated.Set();
                }
            };
            testStore.InnerStringSelector.SelectorUpdated += (_, newState) =>
            {
                if (newState == expectedState)
                {
                    outerSelectorUpdated.Set();
                }
            };
            sliceTestStore.Dispatch(sliceTestStore.AppendSliceString, "a");
            Assert.True(innerSelectorUpdated.WaitOne(millisecondsTimeout: 2000));
            Assert.True(outerSelectorUpdated.WaitOne(millisecondsTimeout: 2000));

            innerSelectorUpdated.Reset();
            outerSelectorUpdated.Reset();

            expectedState = "a_b";
            testStore.Dispatch(testStore.AppendInnerString, "_b");
            Assert.True(innerSelectorUpdated.WaitOne(millisecondsTimeout: 2000));
            Assert.True(outerSelectorUpdated.WaitOne(millisecondsTimeout: 2000));
        }

        [Fact]
        public void Test_Slice_RecordProp()
        {
            var innerSelectorUpdated = new AutoResetEvent(false);
            var outerSelectorUpdated = new AutoResetEvent(false);
            var expectedIntState = 1;
            Assert.Equal(0, sliceTestStore.InnerPropAsIntSelector);
            Assert.Equal('d', testStore.LastCharacterOfPropStringSelector);

            sliceTestStore.InnerPropAsIntSelector.SelectorUpdated += (_, newState) =>
            {
                if (newState == expectedIntState)
                {
                    innerSelectorUpdated.Set();
                }
            };
            testStore.LastCharacterOfPropStringSelector.SelectorUpdated += (_, newState) =>
            {
                if (newState == expectedIntState.ToString().Last())
                {
                    outerSelectorUpdated.Set();
                }
            };
            sliceTestStore.Dispatch(sliceTestStore.IncreaseCountStringProp);
            Assert.True(innerSelectorUpdated.WaitOne(millisecondsTimeout: 2000));
            Assert.True(outerSelectorUpdated.WaitOne(millisecondsTimeout: 2000));

            innerSelectorUpdated.Reset();
            outerSelectorUpdated.Reset();

            expectedIntState = 2;
            sliceTestStore.Dispatch(sliceTestStore.IncreaseCountStringProp);
            Assert.True(innerSelectorUpdated.WaitOne(millisecondsTimeout: 2000));
            Assert.True(outerSelectorUpdated.WaitOne(millisecondsTimeout: 2000));

            expectedIntState = 0;
            sliceTestStore.Dispatch(testStore.ResetInnerPropStringTo0);
            Assert.True(innerSelectorUpdated.WaitOne(millisecondsTimeout: 2000));
            Assert.True(outerSelectorUpdated.WaitOne(millisecondsTimeout: 2000));
        }

        [Theory]
        [Repeat(3)]
        public void Test_MultipleThreads(int _)
        {
            var selectorHasTheRightValue = new AutoResetEvent(false);
            const int alphabetLength = 26;
            testStore.InnerStringSelector.SelectorUpdated += (_, newState) =>
            {
                if(newState.Distinct().Count() == alphabetLength)
                {
                    output.WriteLine(newState);
                    selectorHasTheRightValue.Set();
                }
            };

            var singleLetterTasks = 
                Enumerable.Range(0, alphabetLength)
                          .Select(letterIdx => new Task(() => testStore.Dispatch(testStore.AppendInnerString, 
                                                                                 ((char)('a' + letterIdx)).ToString())));
            foreach (var singleLetterTask in singleLetterTasks)
            {
                singleLetterTask.Start();
            }
            Assert.True(selectorHasTheRightValue.WaitOne(millisecondsTimeout: 2000));
        }

        [Theory]
        [Repeat(3)]
        public void Test_ContinouslyFlips(int _)
        {
            var selectorHasTheRightValue = new AutoResetEvent(false);
            var oldState = (bool)testStore.FlippedBoolSelector;
            const int totalNrOfTasks = 16;
            int numberOfUpdates = 0;
            testStore.FlippedBoolSelector.SelectorUpdated += (_, newState) =>
            {
                numberOfUpdates++;
                if (newState == oldState)
                {
                    throw new InvalidDataException("Expected flipped values");
                }
                oldState = newState;

                if (numberOfUpdates == totalNrOfTasks)
                {
                    selectorHasTheRightValue.Set();
                }
            };

            for(int i=0; i<totalNrOfTasks; i++)
            {
                Task.Run(() => testStore.Dispatch(testStore.FlipBool));
            }
            Assert.True(selectorHasTheRightValue.WaitOne(millisecondsTimeout: 2000));
        }

        [Theory]
        [Repeat(3)]
        public void Test_ContinouslyIncreases(int _)
        {
            testStore = new TestStore(new() { NumProp = 1 });
            var selectorHasTheRightValue = new AutoResetEvent(false);
            const int totalNrOfTasks = 16;
            int numberOfUpdates = 0;
            testStore.DoubleNumSelector.SelectorUpdated += (_, newState) =>
            {
                numberOfUpdates++;
                var expectedValue = 4 * numberOfUpdates + 2;
                if(newState != expectedValue)
                {
                    throw new InvalidDataException("Possible race condition");
                }

                if (numberOfUpdates == totalNrOfTasks)
                {
                    selectorHasTheRightValue.Set();
                }
            };

            for (int i = 0; i < totalNrOfTasks; i++)
            {
                Task.Run(() => testStore.Dispatch(testStore.IncreaseNum, 2));
            }
            Assert.True(selectorHasTheRightValue.WaitOne(millisecondsTimeout: 2000));
        }
    }
}