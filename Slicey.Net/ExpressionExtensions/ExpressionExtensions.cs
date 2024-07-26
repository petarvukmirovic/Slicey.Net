using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Slicey.Net.Test.Expressions")]
namespace Slicey.Net.ExpressionExtensions
{
    internal static class ExpressionExtensions
    {
        public static bool IsIdentity<TArg, TResult>(this Expression<Func<TArg, TResult>> lambda)
        {
            var parameter = lambda.Parameters.Single();
            var body = lambda.Body;
            return body is ParameterExpression parameterExpression && 
                   parameterExpression.Name == parameter.Name;
        }

        public static bool IsPropertyAccessorChain<TArg, TResult>(this Expression<Func<TArg, TResult>> lambda, bool requireWriteable = false)
        {
            bool IsPropertyOrField(MemberInfo memberInfo) =>
                memberInfo is FieldInfo || memberInfo is PropertyInfo;

            bool IsSettable(MemberInfo memberInfo)
            {
                if (memberInfo is FieldInfo fieldInfo)
                {
                    return !fieldInfo.IsInitOnly && !fieldInfo.IsLiteral;
                }
                else if (memberInfo is PropertyInfo propertyInfo)
                {
                    return propertyInfo.CanWrite && propertyInfo.SetMethod != null && !propertyInfo.SetMethod.IsPrivate;
                }
                else
                {
                    return false;
                }
            }

            var parameter = lambda.Parameters.Single();
            var body = lambda.Body;
            var isPropertyAccessorChain = false;
            if(body is MemberExpression memberAccessChain)
            {
                if(!requireWriteable || IsSettable(memberAccessChain.Member))
                {
                    while (body is MemberExpression memberExpresion && IsPropertyOrField(memberExpresion.Member))
                    {
                        body = ((MemberExpression)body).Expression;
                    }
                    isPropertyAccessorChain = body is ParameterExpression parameterExpression && 
                                              parameterExpression.Name == parameter.Name;
                }
            }
            return isPropertyAccessorChain;
        }

        public static Expression<Func<TRoot, TTarget>> ConcatenateProperyAccessors<TRoot, TMiddle, TTarget>(
            this Expression<Func<TRoot, TMiddle>> rootSelector, 
            Expression<Func<TMiddle, TTarget>> childSelector)
        {
            bool rootIsIdentity = rootSelector.IsIdentity();
            bool childIsIdentity = childSelector.IsIdentity();
            if (rootIsIdentity && childIsIdentity)
            {
                return (Expression<Func<TRoot, TTarget>>)(dynamic)rootSelector;
            }
            else if(rootIsIdentity && !childIsIdentity)
            {
                return (Expression<Func<TRoot, TTarget>>)(dynamic)childSelector;
            }
            else if(childIsIdentity && !rootIsIdentity)
            {
                return (Expression<Func<TRoot, TTarget>>)(dynamic)rootSelector;
            }
            else
            {
                return HandleNonTrivialConcatenation(rootSelector, childSelector);
            }
        }

        private static Expression<Func<TRoot, TTarget>> HandleNonTrivialConcatenation<TRoot, TMiddle, TTarget>(
            Expression<Func<TRoot, TMiddle>> rootSelector, 
            Expression<Func<TMiddle, TTarget>> childSelector)
        {
            if(rootSelector.IsPropertyAccessorChain(requireWriteable:false) &&
               childSelector.IsPropertyAccessorChain(requireWriteable:true))
            {
                var members = GetMembersInChain(childSelector);
                Expression concatenatedMemebers = ConcatenateMembersToBody(rootSelector.Body, members);
                return (Expression<Func<TRoot, TTarget >>)Expression.Lambda(concatenatedMemebers, rootSelector.Parameters);
            }
            else
            {
                var err = "Could not concatenate expressions -- both need to be property chains and the second must be writeable";
                throw new InvalidOperationException(err);
            }
        }

        private static Expression ConcatenateMembersToBody(Expression body, Stack<MemberInfo> members)
        {
            while(members.Count != 0)
            {
                var member = members.Pop();
                body = Expression.MakeMemberAccess(body, member);
            }
            return body;
        }

        public static Stack<MemberInfo> GetMembersInChain<Troot, TTarget>(this Expression<Func<Troot, TTarget>> lambda)
        {
            Stack<MemberInfo> members = [];
            var body = lambda.Body;
            while (body != null && body is MemberExpression memberExpresion)
            {
                members.Push(memberExpresion.Member);
                body = memberExpresion.Expression;
            }
            return members;
        }

        public static void UpdateReference<TRoot, TResult>(this Expression<Func<TRoot, TResult>> stateSliceSelector, ref TRoot root, TResult newValue)
        {
            if(stateSliceSelector.IsIdentity() && newValue is TRoot newValueAsRoot)
            {
                root = newValueAsRoot;
            }
            else if(stateSliceSelector.IsPropertyAccessorChain(requireWriteable:true))
            {
                var newValueConstant = Expression.Constant(newValue);
                var oldBody = stateSliceSelector.Body;
                var newBody = Expression.Assign(oldBody, newValueConstant);
                var assingmentExpression = stateSliceSelector.Update(newBody, stateSliceSelector.Parameters);

                assingmentExpression.Compile().Invoke(root);
            }
            else
            {
                throw new InvalidOperationException("Could not update reference -- make sure types are compatible and that target is assignable");
            }
        }
    }
}
