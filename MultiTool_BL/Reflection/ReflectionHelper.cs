using System;
using System.Reflection;

public static class ReflectionHelper
{
    /// <summary>
    /// Determines if <paramref name="actualType"/> implements <paramref name="targetType"/>
    /// </summary>
    /// <param name="targetType">Type of the interface to implement</param>
    /// <param name="actualType">Type of the object</param>
    /// <returns>True if the object implements the <see cref="Type"/>, false otherwise</returns>
    public static bool Implements(Type targetType, Type actualType)
    {
        if (actualType == targetType)
        {
            return true;
        }
        else
        {
            Type[] interfaces = actualType.GetInterfaces();
            for (int i = 0; i < interfaces.Length; i++)
            {
                if (interfaces[i].Equals(targetType))
                {
                    return true;
                }
            }
            return false;
        }
    }

    /// <summary>
    /// Determines if <typeparamref name="ActualType"/> implements <typeparamref name="TargetType"/>
    /// </summary>
    /// <typeparam name="TargetType">Type of the interface to implement</typeparam>
    /// <typeparam name="ActualType">Type of the object</typeparam>
    /// <returns>True if the object implements the <see cref="Type"/>, false otherwise</returns>
    public static bool Implements<TargetType, ActualType>()
    {
        return Implements(typeof(TargetType), typeof(ActualType));
    }

    /// <summary>
    /// Determines if <paramref name="actualType"/> implements <typeparamref name="TargetType"/>
    /// </summary>
    /// <typeparam name="TargetType">Type of the interface to implement</typeparam>
    /// <param name="actualType">Type of the object</param>
    /// <returns>True if the object implements the <typeparamref name="TargetType"/>, false otherwise</returns>
    public static bool Implements<TargetType>(Type actualType)
    {
        return Implements(typeof(TargetType), actualType);
    }

    public static bool IsPrimitiveType(Type t , bool includeString = true)
    {
        return t.IsPrimitive || (t == typeof(string) && includeString);
    }

    public static bool IsPrimitiveType<T>(bool includeString = true)
    {
        return IsPrimitiveType(typeof(T), includeString);
    }

    public static PropertyInfo[] GetPropertyInfos<T>()
    {
        return typeof(T).GetProperties();
    }

    public static PropertyInfo[] GetPropertyInfos(Type type)
    {
        return type.GetProperties();
    }
}