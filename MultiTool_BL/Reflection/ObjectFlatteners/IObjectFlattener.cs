using System;

namespace Multitool.Reflection.ObjectFlatteners
{
    public interface IObjectFlattener<FlatType>
    {
        FlatType Flatten(object o, Type objectType);
    }
}
