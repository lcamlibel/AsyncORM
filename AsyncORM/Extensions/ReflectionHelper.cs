using System;
using System.Linq.Expressions;
using System.Reflection;

namespace AsyncORM.Extensions
{
    internal static class ReflectionHelper
    {
        internal static Func<object, object> BuildGetAccessor(MethodInfo method)
        {
            ParameterExpression obj = Expression.Parameter(typeof (object), "o");

            Expression<Func<object, object>> expr =
                Expression.Lambda<Func<object, object>>(
                    Expression.Convert(Expression.Call(Expression.Convert(obj, method.DeclaringType), method),
                                       typeof (object)), obj);

            return expr.Compile();
        }

        internal static Action<object, object> BuildSetAccessor(MethodInfo method)
        {
            ParameterExpression obj = Expression.Parameter(typeof (object), "o");
            ParameterExpression value = Expression.Parameter(typeof (object));

            Expression<Action<object, object>> expr =
                Expression.Lambda<Action<object, object>>(
                    Expression.Call(Expression.Convert(obj, method.DeclaringType), method,
                                    Expression.Convert(value, method.GetParameters()[0].ParameterType)), obj, value);

            return expr.Compile();
        }
    }
}