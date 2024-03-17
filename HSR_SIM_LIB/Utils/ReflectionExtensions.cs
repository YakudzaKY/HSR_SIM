using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class ReflectionExtensions
{   
    public static IEnumerable<MethodInfo> GetExtensionMethods(this Type type, Assembly extensionsAssembly)
    {
        var query = from t in extensionsAssembly.GetTypes()
            where !t.IsGenericType && !t.IsNested
            from m in t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            where m.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false)
            where m.GetParameters()[0].ParameterType == type
            select m;

        return query;
    }

    public static MethodInfo GetExtensionMethod(this Type type, Assembly extensionsAssembly, string name)
    {
        return type.GetExtensionMethods(extensionsAssembly).FirstOrDefault(m => m.Name == name);
    }

    public static MethodInfo GetExtensionMethod(this Type type, Assembly extensionsAssembly, string name, Type[] types)
    {
        var methods = (from m in type.GetExtensionMethods(extensionsAssembly)
            where m.Name == name
                  && m.GetParameters().Count() == types.Length + 1 // + 1 because extension method parameter (this)
            select m).ToList();

        if (!methods.Any())
        {
            return default(MethodInfo);
        }

        if (methods.Count() == 1)
        {
            return methods.First();
        }

        foreach (var methodInfo in methods)
        {
            var parameters = methodInfo.GetParameters();

            bool found = true;
            for (byte b = 0; b < types.Length; b++)
            {
                found = true;
                if (parameters[b].GetType() != types[b])
                {
                    found = false;
                }
            }

            if (found)
            {
                return methodInfo;
            }
        }

        return default(MethodInfo);
    }
}