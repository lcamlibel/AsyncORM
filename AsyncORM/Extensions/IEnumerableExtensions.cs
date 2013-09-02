using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using AsyncORM.Extensions;

namespace AsyncORM
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<TResult> MapFromTo<TSource, TResult>(this IEnumerable sourceEnumerable) where TSource: class,new() where TResult:class, new()
        {
            var sourceInstanceSample = Activator.CreateInstance<TSource>();
            var destinationInstanceSample = Activator.CreateInstance<TResult>();

            Type sourceType = sourceInstanceSample.GetType();
            Type destinationType = destinationInstanceSample.GetType();
            bool isSourceDynamic = (sourceInstanceSample as IDictionary<String, Object>) != null;
            bool isDestinationDynamic = (destinationInstanceSample as IDictionary<String, Object>) != null;


            foreach (TSource sourceInstance in sourceEnumerable)
            {
                if (!isSourceDynamic && !isDestinationDynamic)
                {
                    IEnumerable<PropertyInfo> sourceProps = GetProperties(false, sourceType, sourceInstanceSample);
                    IEnumerable<PropertyInfo> destinationProps = GetProperties(false, destinationType,
                                                                               destinationInstanceSample);

                    yield return MapObjectToObject<TSource, TResult>(sourceInstance, sourceProps, destinationProps);
                }
                else if (isSourceDynamic && !isDestinationDynamic)
                {
                    IEnumerable<PropertyInfo> destinationProps = GetProperties(false, destinationType,
                                                                               destinationInstanceSample);
                    yield return
                        MapDynamicToObject<TResult>((sourceInstance as IDictionary<String, Object>), destinationProps);
                }
                else if (!isSourceDynamic && isDestinationDynamic)
                {
                    IEnumerable<PropertyInfo> sourceProps = GetProperties(false, sourceType, sourceInstanceSample);
                    yield return MapObjectToDynamic(sourceInstance, sourceProps);
                }
                else
                {
                    throw new Exception("please use concrete object type");
                }
            }
        }

        private static IEnumerable<PropertyInfo> GetProperties(bool isDynamic, Type type, object instance)
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

        private static TResult MapObjectToObject<TSource, TResult>(TSource sourceInstance,
                                                                   IEnumerable<PropertyInfo> sourceProps,
                                                                   IEnumerable<PropertyInfo> destinationProps)
        {
            var destinationInstance = Activator.CreateInstance<TResult>();
            foreach (PropertyInfo sourceProp in sourceProps)
            {
                foreach (PropertyInfo destinationProp in destinationProps)
                    if ((sourceProp.Name.Equals(destinationProp.Name, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        var getAccessor = ReflectionHelper.BuildGetAccessor(sourceProp.GetGetMethod());
                        var setAccessor = ReflectionHelper.BuildSetAccessor(destinationProp.GetSetMethod());

                        setAccessor(destinationInstance, getAccessor(sourceInstance));
                        break;
                    }
            }
            return destinationInstance;
        }

        private static TResult MapDynamicToObject<TResult>(IEnumerable<KeyValuePair<string, object>> sourceProps,
                                                           IEnumerable<PropertyInfo> destinationProps)
        {
            var destinationInstance = Activator.CreateInstance<TResult>();
            foreach (var  sourceProp in sourceProps)
            {
                foreach (PropertyInfo destinationProp in destinationProps)
                    if ((sourceProp.Key.Equals(destinationProp.Name, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        var setAccessor = ReflectionHelper.BuildSetAccessor(destinationProp.GetSetMethod());

                        setAccessor(destinationInstance, sourceProp.Value);
                        break;
                    }
            }
            return destinationInstance;
        }

        private static dynamic MapObjectToDynamic<TSource>(TSource sourceInstance,
                                                                    IEnumerable<PropertyInfo> sourceProps)
        {
            dynamic destination = new ExpandoObject();
            var destinationInstance = destination as IDictionary<string, object>;

            foreach (PropertyInfo sourceProp in sourceProps)
            {
                var getAccessor = ReflectionHelper.BuildGetAccessor(sourceProp.GetGetMethod());
              
                destinationInstance.Add(sourceProp.Name, getAccessor(sourceInstance));
                break;
            }
            return destination;
        }
    }
}