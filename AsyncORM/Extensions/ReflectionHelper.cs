using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq;
using System.Runtime.CompilerServices;
namespace AsyncORM.Extensions
{
    public static class ReflectionHelper
    {

        public static bool IsAnonymousType(this Type type)
        {
            bool hasCompilerGeneratedAttribute = type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Count() > 0;
            bool nameContainsAnonymousType = type.FullName.Contains("AnonymousType");
            return hasCompilerGeneratedAttribute && nameContainsAnonymousType;
        }
        public static bool IsDynamicType(this object type)
        {
            return type is IDynamicMetaObjectProvider || type is IDictionary<string, Object>;
        }

        public static Func<object, object> BuildGetAccessor(MethodInfo method)
        {
            ParameterExpression obj = Expression.Parameter(typeof(object), "o");

            Expression<Func<object, object>> expr =
                Expression.Lambda<Func<object, object>>(
                    Expression.Convert(Expression.Call(Expression.Convert(obj, method.DeclaringType), method),
                                       typeof(object)), obj);

            return expr.Compile();
        }

        public static Action<object, object> BuildSetAccessor(MethodInfo method)
        {
            ParameterExpression obj = Expression.Parameter(typeof(object), "o");
            ParameterExpression value = Expression.Parameter(typeof(object));

            Expression<Action<object, object>> expr =
                Expression.Lambda<Action<object, object>>(
                    Expression.Call(Expression.Convert(obj, method.DeclaringType), method,
                                    Expression.Convert(value, method.GetParameters()[0].ParameterType)), obj, value);

            return expr.Compile();
        }
        public static AsyncColumnMapAttribute GetAttribute(MemberInfo memberInfo, bool isDynamic)
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
        public static IEnumerable<PropertyInfo> GetProperties(bool isDynamic, Type type, object instance)
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

        public static TResult MapObjectToObject<TSource, TResult>(TSource sourceInstance,
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

        public static TResult MapDynamicToObject<TResult>(IEnumerable<KeyValuePair<string, object>> sourceProps,
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
        public static TResult MapDynamicToObject<TResult>(ExpandoObject expandoObject)
        {
            IEnumerable<KeyValuePair<string, object>> sourceProps = expandoObject as IDictionary<string, object>;

            var destinationInstance = Activator.CreateInstance<TResult>();
            var destinationProps = typeof(TResult).GetProperties();
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
        public static dynamic MapObjectToDynamic<TSource>(TSource sourceInstance, IEnumerable<PropertyInfo> sourceProps)
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
        public static void SetValue(object inputObject, PropertyInfo propertyInfo, object propertyVal)
        {
            Type propertyType = propertyInfo.PropertyType;
            var targetType = IsNullableType(propertyType) ? Nullable.GetUnderlyingType(propertyType) : propertyInfo.PropertyType;
            propertyVal = Convert.ChangeType(propertyVal, targetType);
            propertyInfo.SetValue(inputObject, propertyVal, null);
        }
        private static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}