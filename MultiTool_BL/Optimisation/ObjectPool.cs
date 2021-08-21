using System;
using System.Collections.Generic;
using System.Reflection;

namespace Multitool.Optimisation
{
    public class ObjectPool<T> where T : class, IPoolableObject
    {
        public const int DEFAULT_CAPACITY = 10;

        private object syncObject = new object();
        private Stack<T> freePool;
        private List<PropertyInfo> propertyInfos = new List<PropertyInfo>();

        #region constructors
        /// <summary>Initialise the pool with the <see cref="DEFAULT_CAPACITY"/> capacity</summary>
        public ObjectPool()
        {
            freePool = new Stack<T>(DEFAULT_CAPACITY);
            GetProperties();
            PreloadPool(DEFAULT_CAPACITY);
        }

        /// <summary>Initialise the pool with the <paramref name="capacity"/></summary>
        /// <param name="capacity">Internal list initial capacity</param>
        public ObjectPool(int capacity)
        {
            freePool = new Stack<T>(capacity);
            GetProperties();
            PreloadPool(capacity);
        }

        /// <summary>
        /// Initialise the pool with the <see cref="DEFAULT_CAPACITY"/>, initialising the <typeparamref name="T"/>
        /// objects with the <paramref name="constructorParameters"/> parameters
        /// </summary>
        /// <param name="constructorParameters">Parameters to initialise the <typeparamref name="T"/> objects with</param>
        public ObjectPool(params object[] constructorParameters)
        {
            freePool = new Stack<T>(DEFAULT_CAPACITY);
            GetProperties();
            PreloadPool(DEFAULT_CAPACITY, constructorParameters);
        }

        /// <summary>
        /// Initialise the pool with the specified <paramref name="capacity"/>, initialising the <typeparamref name="T"/>
        /// objects with the <paramref name="constructorParameters"/> parameters
        /// </summary>
        /// <param name="capacity">Internal list initial capacity</param>
        /// <param name="constructorParameters">Parameters to initialise the <typeparamref name="T"/> objects with</param>
        public ObjectPool(int capacity, params object[] constructorParameters)
        {
            freePool = new Stack<T>(capacity);
            GetProperties();
            PreloadPool(capacity, constructorParameters);
        }
        #endregion

        /// <summary>
        /// Gets an object from the pool.
        /// <para>
        /// <paramref name="ctorParams"/> represents the parameters of the class constructor 
        /// (if the constructor is parameter-less put null). It needs to be indexed as the 
        /// actual constructor parameter. See <seealso cref="Type.GetConstructor(Type[])"/>
        /// </para>
        /// </summary>
        /// <param name="ctorParams">Parameters to feed to the class constructor of the object.</param>
        /// <returns></returns>
        public T GetObject(params object[] ctorParams)
        {
            lock (syncObject)
            {
                // if free pool has some
                if (freePool.Count > 0)
                {
                    T obj = freePool.Pop();
                    obj.InUse = true;

                    if (ctorParams.Length > 0)
                    {
                        for (int i = 0; i < ctorParams.Length; i++)
                        {
                            Type paramType = ctorParams[i].GetType();
                            for (int j = 0; j < propertyInfos.Count; j++)
                            {
                                if (propertyInfos[j].PropertyType.IsAssignableFrom(paramType) && propertyInfos[j].SetMethod != null)
                                {
                                    propertyInfos[j].SetValue(obj, ctorParams[i]);
                                    break;
                                }
                            }
                        }
                    }

                    return obj;
                }
                else // else build object and send it
                {
                    T obj = BuildObject(ctorParams);
                    obj.InUse = true;
                    return obj;
                }
            }
        }

        private T BuildObject(object[] ctorParams)
        {
            T o;
            if (ctorParams != null && ctorParams.Length > 0)
            {
                Type[] paramTypes = GetParamsTypes(ctorParams);
                ConstructorInfo constructorInfo = typeof(T).GetConstructor(paramTypes);
                if (constructorInfo == null)
                {
                    throw new ArgumentException("Could not find the constructor matching the given parameters", nameof(ctorParams));
                }
                o = constructorInfo.Invoke(ctorParams) as T;
            }
            else // parameter-less constructor
            {
                ConstructorInfo constructorInfo = typeof(T).GetConstructor(new Type[0]);
                if (constructorInfo == null)
                {
                    throw new ArgumentException("Could not find the parameter-less constructor for ", typeof(T).FullName);
                }
                o = constructorInfo.Invoke(null) as T;
            }
            o.Free += OnPoolObjectFreed;
            return o;
        }

        private void PreloadPool(int capacity, object[] ctorParameters)
        {
            for (int i = 0; i < capacity; i++)
            {
                freePool.Push(BuildObject(ctorParameters));
            }
        }

        private void PreloadPool(int capacity)
        {
            PreloadPool(capacity, null);
        }

        private void GetProperties()
        {
            PropertyInfo[] infos = typeof(T).GetProperties();
            for (int i = 0; i < infos.Length; i++)
            {
                if (infos[i].Name != "InUse")
                {
                    propertyInfos.Add(infos[i]);
                }
            }
        }

        private Type[] GetParamsTypes(object[] parameters)
        {
            Type[] paramTypes = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                Type paramType = parameters[i].GetType();
                if (paramType != null)
                {
                    paramTypes[i] = paramType;
                }
            }
            return paramTypes;
        }

        private void OnPoolObjectFreed(object sender)
        {
            lock (syncObject)
            {
                T o = (T)sender;
                if (o.InUse)
                {
                    throw new InvalidOperationException("Object is not in a valid state (still in use)");
                }
                freePool.Push(o);
            }
        }
    }
}
