using System.Collections.Generic;

namespace BusinessLayer.Reflection.ObjectFlatteners
{
    public interface IObjectFlattener<FlatType>
    {
        FlatType Flatten<T>(T o) where T : class;
    }
}
