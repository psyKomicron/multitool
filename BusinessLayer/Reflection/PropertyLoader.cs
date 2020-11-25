using System;
using System.Collections.Generic;
using System.Reflection;

namespace BusinessLayer.Reflection
{
    internal class PropertyLoader : IPropertyLoader
    {
        public DtoType Load<DtoType>(Dictionary<string, string> dictionnary) where DtoType : class, new()
        {
            DtoType dto = new DtoType();
            PropertyInfo[] properties = GetProperties<DtoType>();

            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];

                if (dictionnary.ContainsKey(property.Name))
                {
                    string propValue = dictionnary[property.Name];
                    Type type = property.PropertyType;

                    var value = Convert.ChangeType(propValue, type);
                    property.SetValue(dto, value);
                }
            }

            return dto;
        }

        private PropertyInfo[] GetProperties<T>() where T : class
        {
            return typeof(T).GetProperties();
        }
    }
}
