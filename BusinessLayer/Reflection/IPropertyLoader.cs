using System.Collections.Generic;

namespace BusinessLayer.Reflection
{
    internal interface IPropertyLoader
    {
        DtoType Load<DtoType>(Dictionary<string, string> dictionnary) where DtoType : class, new();
    }
}
