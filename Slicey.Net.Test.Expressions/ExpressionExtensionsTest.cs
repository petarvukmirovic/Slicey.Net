using System.Linq.Expressions;
using Slicey.Net.ExpressionExtensions;
using static Slicey.Net.Test.Expressions.ExpressionTestClass;
using static Slicey.Net.Test.Expressions.ExpressionTestClass.NestedClass;

namespace Slicey.Net.Test.Expressions
{
    public class ExpressionExtensionsTest
    {
        [Theory]
        [MemberData(nameof(GetIdentityCheckData))]
        public void TestIdentity(Expression<Func<int, int>> lambda, bool expectedAnswer)
        {
            Assert.Equal(expectedAnswer, lambda.IsIdentity());
        }

        [Theory]
        [MemberData(nameof(GetMemberAccessData), parameters:[false,false])]
        public void TestMemberAccess_ObjectType_DoNotRequireSettable(Expression<Func<ExpressionTestClass, object>> lambda, bool expectedAnswer)
        {
            Assert.Equal(expectedAnswer, lambda.IsPropertyAccessorChain(requireWriteable: false));
        }

        [Theory]
        [MemberData(nameof(GetMemberAccessData), parameters:[false,true])]
        public void TestMemberAccess_ObjectType_RequireSettable(Expression<Func<ExpressionTestClass, object>> lambda, bool expectedAnswer)
        {
            Assert.Equal(expectedAnswer, lambda.IsPropertyAccessorChain(requireWriteable: true));
        }

        [Theory]
        [MemberData(nameof(GetMemberAccessData), parameters: [true,false])]
        public void TestMemberAccess_PrimitiveType_DoNotRequireSettable(Expression<Func<ExpressionTestClass, int>> lambda, bool expectedAnswer)
        {
            Assert.Equal(expectedAnswer, lambda.IsPropertyAccessorChain(requireWriteable: false));
        }

        [Theory]
        [MemberData(nameof(GetMemberAccessData), parameters: [true,true])]
        public void TestMemberAccess_PrimitiveType_RequireSettable(Expression<Func<ExpressionTestClass, int>> lambda, bool expectedAnswer)
        {
            Assert.Equal(expectedAnswer, lambda.IsPropertyAccessorChain(requireWriteable: true));
        }

        [Fact]
        public void Test_UpdatingAtRoot()
        {
            var testObject = new ExpressionTestClass()
            {
                SettableProperty = "TEST-1"
            };
            Expression<Func<ExpressionTestClass, ExpressionTestClass>> idFunction = x => x;
            idFunction.UpdateReference(ref testObject, new ExpressionTestClass() { SettableProperty = "TEST-2" });
            Assert.Equal("TEST-2", testObject.SettableProperty);
        }

        [Fact]
        public void Test_UpdatingNested()
        {
            var testObject = new ExpressionTestClass()
            {
                SettableProperty = "TEST-1",
                NestedProperty = new() { NestedRecord = new(1) }
            };
            Expression<Func<ExpressionTestClass, int>> idFunction = x => x.NestedProperty.NestedRecord.recordProperty;
            idFunction.UpdateReference(ref testObject, 2);
            Assert.Equal(2, testObject.NestedProperty.NestedRecord.recordProperty);
            Assert.Equal("TEST-1", testObject.SettableProperty);
        }

        [Fact]
        public void Test_UpdatingReadonly()
        {
            var testObject = new ExpressionTestClass()
            {
                SettableProperty = "TEST-1",
                NestedProperty = new() { NestedRecord = new(1) }
            };
            Expression<Func<ExpressionTestClass, int>> idFunction = x => x.NestedProperty.NestedReadOnlyProp;
            Assert.Throws<InvalidOperationException>(() => idFunction.UpdateReference(ref testObject, 123));
        }

        [Fact]
        public void Test_Concatenate_BothId()
        {
            Expression<Func<int, int>> parentExpression = x => x;
            Expression<Func<int, int>> childExpression = x => x;
            var concatenated = parentExpression.ConcatenateProperyAccessors(childExpression);
            Assert.True(concatenated.Body is ParameterExpression);
        }

        [Fact]
        public void Test_Concatenate_OneId()
        {
            Expression<Func<ExpressionTestClass, ExpressionTestClass>> parentExpression = x => x;
            Expression<Func<ExpressionTestClass, int>> childExpression = x => x.NestedProperty.NestedRecord.recordProperty;
            var concatenatedLeftRight = parentExpression.ConcatenateProperyAccessors(childExpression);
            Assert.Equal(concatenatedLeftRight, childExpression);

            var concatenatedRightLeft = childExpression.ConcatenateProperyAccessors(x => x);
            Assert.Equal(concatenatedRightLeft, childExpression);
        }

        [Fact]
        public void Test_Concatenate_RealConcatenation()
        {
            Expression<Func<ExpressionTestClass, NestedClass>> parentExpression = x => x.NestedProperty;
            Expression<Func<NestedClass, int>> childExpression = x => x.NestedRecord.recordProperty;
            var concatenated = parentExpression.ConcatenateProperyAccessors(childExpression);

            var members = concatenated.GetMembersInChain();
            Assert.Equal(
                [
                    nameof(ExpressionTestClass.NestedProperty),
                    nameof(NestedClass.NestedRecord),
                    nameof(NestedNestedRecord.recordProperty)
                ],
                members.Select(m => m.Name));
        }

        public static IEnumerable<object[]> GetIdentityCheckData()
        {
            int externalVal = 1;
            return 
            [
                [(Expression<Func<int, int>>)((int x) => x), true],
                [(Expression<Func<int, int>>)((int x) => externalVal), false],
                [(Expression<Func<int, int>>)((int x) => 2), false],
                [(Expression<Func<int, int>>)((int x) => x+1), false],
                [(Expression<Func<int, int>>)((int x) => x.CompareTo(x)), false]
            ];
        }

        public static IEnumerable<object[]> GetMemberAccessData(bool primitiveExpressions, bool requireSettable)
        {
            ExpressionTestClass externalVariable = new();
            IEnumerable<object[]> objectTypedExpressions = 
            [
                [(Expression<Func<ExpressionTestClass, object>>)((ExpressionTestClass x) => x), false],
                [(Expression<Func<ExpressionTestClass, object>>)((ExpressionTestClass x) => x.ToString()!), false],
                [(Expression<Func<ExpressionTestClass, object>>)((ExpressionTestClass x) => x.SettableProperty.ToUpper()), false],
                [(Expression<Func<ExpressionTestClass, object>>)((ExpressionTestClass x) => externalVariable.SettableProperty), false],
                [(Expression<Func<ExpressionTestClass, object>>)((ExpressionTestClass x) => x.SettableProperty), true],
                [(Expression<Func<ExpressionTestClass, object>>)((ExpressionTestClass x) => x.NestedProperty.NestedReadonlyField), !requireSettable],
                [(Expression<Func<ExpressionTestClass, object>>)((ExpressionTestClass x) => ExpressionTestClass.NestedClass.NestedConst), false]
            ];
            IEnumerable<object[]> primitiveTypedExpressions =
            [
                [(Expression<Func<ExpressionTestClass, int>>)((ExpressionTestClass x) => x.ToString()!.Length), false],
                [(Expression<Func<ExpressionTestClass, int>>)((ExpressionTestClass x) => x.ReadonlyProperty), !requireSettable],
                [(Expression<Func<ExpressionTestClass, int>>)((ExpressionTestClass x) => x.NestedProperty.NestedRecord.recordProperty), true], // init properties are runtime settable
                [(Expression<Func<ExpressionTestClass, int>>)((ExpressionTestClass x) => x.ReadonlyProperty), !requireSettable],
                [(Expression<Func<ExpressionTestClass, int>>)((ExpressionTestClass x) => externalVariable.ReadonlyProperty), false],
                [(Expression<Func<ExpressionTestClass, int>>)((ExpressionTestClass x) => x.NestedProperty.NestedReadOnlyProp), !requireSettable],

            ];
            return primitiveExpressions ? primitiveTypedExpressions : objectTypedExpressions; 
            
        }
    }
}