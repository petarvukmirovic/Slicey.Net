namespace Slicey.Net.Test.Expressions
{
    public class ExpressionTestClass
    {
        public class NestedClass
        {
            public record NestedNestedRecord(int recordProperty);

            public readonly string NestedReadonlyField = "";
            public const string NestedConst = "ABC";
            public int NestedReadOnlyProp { get; } = 2;
            public NestedNestedRecord NestedRecord { get; set; } = new NestedNestedRecord(3);
        }

        public string SettableProperty { get; set; } = "";
        public int ReadonlyProperty { get; } = 1;
        public readonly string ReadonlyField = "";
        public NestedClass NestedProperty { get; set; } = new NestedClass();
    }
}
