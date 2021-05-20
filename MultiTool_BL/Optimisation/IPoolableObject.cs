namespace Multitool.Optimisation
{
    public interface IPoolableObject
    {
        /// <summary>
        /// Tells if the <see cref="IPoolableObject"/> is in use and thus cannot be used by the pool.
        /// </summary>
        bool InUse { get; set; }

        /// <summary>
        /// Fired when the object is free to be reused by the <see cref="ObjectPool{T}"/>.
        /// </summary>
        event FreeObjectEventHandler Free;
    }
}
