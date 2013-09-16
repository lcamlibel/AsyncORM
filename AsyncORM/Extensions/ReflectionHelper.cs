using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq;
using System.Runtime.CompilerServices;
namespace AsyncORM.Extensions
{
    internal static class ReflectionHelper
    {

        internal static bool IsAnonymousType(this Type type)
        {
            bool hasCompilerGeneratedAttribute = type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Count() > 0;
            bool nameContainsAnonymousType = type.FullName.Contains("AnonymousType");
            return hasCompilerGeneratedAttribute && nameContainsAnonymousType;
        }
        internal static bool IsDynamicType(this object type)
        {
            return type is IDynamicMetaObjectProvider || type is IDictionary<string, Object>;
        }

        internal static Func<object, object> BuildGetAccessor(MethodInfo method)
        {
            ParameterExpression obj = Expression.Parameter(typeof(object), "o");

            Expression<Func<object, object>> expr =
                Expression.Lambda<Func<object, object>>(
                    Expression.Convert(Expression.Call(Expression.Convert(obj, method.DeclaringType), method),
                                       typeof(object)), obj);

            return expr.Compile();
        }

        internal static Action<object, object> BuildSetAccessor(MethodInfo method)
        {
            ParameterExpression obj = Expression.Parameter(typeof(object), "o");
            ParameterExpression value = Expression.Parameter(typeof(object));

            Expression<Action<object, object>> expr =
                Expression.Lambda<Action<object, object>>(
                    Expression.Call(Expression.Convert(obj, method.DeclaringType), method,
                                    Expression.Convert(value, method.GetParameters()[0].ParameterType)), obj, value);

            return expr.Compile();
        }
        internal static AsyncColumnMapAttribute GetAttribute(MemberInfo memberInfo, bool isDynamic)
        {
            if (isDynamic) return null;
            AsyncColumnMapAttribute attr;
            if (AsyncOrmConfig.EnableParameterCache)
            {
                attr =
                    CacheManager.AttributeCache.GetOrAdd(memberInfo, memberInfo.GetCustomAttribute<AsyncColumnMapAttribute>(false));
            }
            else
            {
                attr = memberInfo.GetCustomAttribute<AsyncColumnMapAttribute>(false);
            }

            return attr;
        }
        internal static IEnumerable<PropertyInfo> GetProperties(bool isDynamic, Type type, object instance)
        {
            IEnumerable<PropertyInfo> properties;
            if (AsyncOrmConfig.EnableParameterCache && !isDynamic)
            {
                properties =
                    CacheManager.ParameterCache.GetOrAdd(type, new Lazy<IEnumerable<PropertyInfo>>(type.GetProperties))
                                .Value;
            }
            else
            {
                properties = instance.GetType().GetProperties();
            }
            return properties;
        }

        internal static TResult MapObjectToObject<TSource, TResult>(TSource sourceInstance,
                                                                   IEnumerable<PropertyInfo> sourceProps,
                                                                   IEnumerable<PropertyInfo> destinationProps)
        {
            var destinationInstance = Activator.CreateInstance<TResult>();
            foreach (PropertyInfo sourceProp in sourceProps)
            {
                foreach (PropertyInfo destinationProp in destinationProps)
                    if ((sourceProp.Name.Equals(destinationProp.Name, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        Func<object, object> getAccessor = ReflectionHelper.BuildGetAccessor(sourceProp.GetGetMethod());
                        Action<object, object> setAccessor =
                            ReflectionHelper.BuildSetAccessor(destinationProp.GetSetMethod());

                        setAccessor(destinationInstance, getAccessor(sourceInstance));
                        break;
                    }
            }
            return destinationInstance;
        }

        internal static TResult MapDynamicToObject<TResult>(IEnumerable<KeyValuePair<string, object>> sourceProps,
                                                           IEnumerable<PropertyInfo> destinationProps)
        {
            var destinationInstance = Activator.CreateInstance<TResult>();
            foreach (var sourceProp in sourceProps)
            {
                foreach (PropertyInfo destinationProp in destinationProps)
                    if ((sourceProp.Key.Equals(destinationProp.Name, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        Action<object, object> setAccessor =
                            ReflectionHelper.BuildSetAccessor(destinationProp.GetSetMethod());

                        setAccessor(destinationInstance, sourceProp.Value);
                        break;
                    }
            }
            return destinationInstance;
        }

        internal static dynamic MapObjectToDynamic<TSource>(TSource sourceInstance, IEnumerable<PropertyInfo> sourceProps)
        {
            dynamic destination = new ExpandoObject();
            var destinationInstance = destination as IDictionary<string, object>;

            foreach (PropertyInfo sourceProp in sourceProps)
            {
                Func<object, object> getAccessor = ReflectionHelper.BuildGetAccessor(sourceProp.GetGetMethod());

                destinationInstance.Add(sourceProp.Name, getAccessor(sourceInstance));
                break;
            }
            return destination;
        }
    }
}