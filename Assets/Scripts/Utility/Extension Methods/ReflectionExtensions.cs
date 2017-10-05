using System.Collections.Generic;

public static class ReflectionHelpers
{
    public static System.Type[] GetAllDerivedTypes_IsSubclassOf(this System.AppDomain aAppDomain, System.Type aType)
    {
        var result = new List<System.Type>();
        var assemblies = aAppDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                if (type.IsSubclassOf(aType))
                    result.Add(type);
            }
        }
        return result.ToArray();
    }

    public static System.Type[] GetAllDerivedTypes_IsAssignableFrom(this System.AppDomain aAppDomain, System.Type aType)
    {
        var result = new List<System.Type>();
        var assemblies = aAppDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                if (aType.IsAssignableFrom(type))
                    result.Add(type);
            }
        }

        return result.ToArray();
    }
}