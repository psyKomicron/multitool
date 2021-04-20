using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MultiToolBusinessLayer.Events
{
    internal class ObjectPool<EventType> where EventType : class, new()
    {
        private List<EventType> eventPool;

        public ObjectPool()
        {
            eventPool = new List<EventType>();
        }

        public ObjectPool(int capacity)
        {
            eventPool = new List<EventType>(capacity);
        }

        public EventType GetEvent(params object[] constructorParameters)
        {
            Type eventType = typeof(EventType);
            Type[] paramTypes = new Type[constructorParameters.Length];

            for (int i = 0; i < constructorParameters.Length; i++)
            {
                Type paramType = constructorParameters[i].GetType();
                if (paramType != null)
                {
                    paramTypes[i] = paramType;
                }
            }

            ConstructorInfo constructorInfo = eventType.GetConstructor(paramTypes);
            if (constructorInfo == null)
            {
                throw new ArgumentException("Could not find the event constructor matching the given parameters", nameof(constructorParameters));
            }



            throw new NotImplementedException();
        }
    }
}
