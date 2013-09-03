using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using AsyncORM.Extensions;

namespace AsyncORM
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<TResult> MapFromTo<TSource, TResult>(this IEnumerable sourceEnumerable)
            where TSource : class, new() where TResult : class, new()
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
                    IEnumerable<PropertyInfo> sourceProps = ReflectionHelper.GetProperties(false, sourceType,
                                                                                           sourceInstanceSample);
                    IEnumerable<PropertyInfo> destinationProps = ReflectionHelper.GetProperties(false, destinationType,
                                                                                                destinationInstanceSample);

                    yield return
                        ReflectionHelper.MapObjectToObject<TSource, TResult>(sourceInstance, sourceProps,
                                                                             destinationProps);
                }
                else if (isSourceDynamic && !isDestinationDynamic)
                {
                    IEnumerable<PropertyInfo> destinationProps = ReflectionHelper.GetProperties(false, destinationType,
                                                                                                destinationInstanceSample);
                    yield return
                        ReflectionHelper.MapDynamicToObject<TResult>((sourceInstance as IDictionary<String, Object>),
                                                                     destinationProps);
                }
                else if (!isSourceDynamic)
                {
                    IEnumerable<PropertyInfo> sourceProps = ReflectionHelper.GetProperties(false, sourceType,
                                                                                           sourceInstanceSample);
                    yield return ReflectionHelper.MapObjectToDynamic(sourceInstance, sourceProps);
                }
                else
                {
                    throw new Exception("please use concrete object type");
                }
            }
        }
    }
}