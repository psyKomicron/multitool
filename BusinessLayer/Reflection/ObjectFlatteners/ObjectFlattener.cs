using System;
using System.Collections;
using System.Reflection;

namespace BusinessLayer.Reflection.ObjectFlatteners
{
    public abstract class ObjectFlattener<FlatType> : IObjectFlattener<FlatType>
    {
        public abstract FlatType Flatten<T>(T o) where T : class;

        protected bool IsPrimitive(Type t)
        {
            return t.IsPrimitive || t == typeof(string);
        }

        protected bool IsList(Type t)
        {
            Type[] interfaces = t.GetInterfaces();
            for (int i = 0; i < interfaces.Length; i++)
            {
                Type type = interfaces[i];
                if (type.Equals(typeof(IList)))
                {
                    return true;
                }
            }
            return false;
        }

        protected PropertyInfo[] GetPropertyInfos<T>()
        {
            return typeof(T).GetProperties();
        }
    }
}
