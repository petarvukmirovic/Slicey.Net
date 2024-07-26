namespace Slicey.Net.Test.Store
{
    public class Record
    {
        public Record(string val)
        {
            StringProp = val;
        }

        public string StringProp { get; set; }
    }


    public class StoreInnerClass 
    {
        public const string InitialInnerRecordString = "inner record";
        public int IntProp { get; set; }
        public string StringProp { get; set; } = "";
        public Record RecordProp { get; set; } = new(InitialInnerRecordString);
    }

    public class StoreDataClass
    {
        public int NumProp { get; set; }
        public bool BoolProp { get; set; }
        public string StringReadonlyProp { get; } = "Readonly";
        public string StringWriteableProp { get; set; } = "";
        public StoreInnerClass InnerProp { get; } = new();
    }
}
