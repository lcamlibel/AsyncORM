using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace AsyncORM
{
    internal class CacheManager
    {
        internal static ConcurrentDictionary<Type, Lazy<IEnumerable<PropertyInfo>>> ParameterCache =
            new ConcurrentDictionary<Type, Lazy<IEnumerable<PropertyInfo>>>();
        internal static ConcurrentDictionary<MemberInfo, AsyncColumnMapAttribute> AttributeCache =
            new ConcurrentDictionary<MemberInfo, AsyncColumnMapAttribute>();
    }
}