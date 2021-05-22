using Multitool.FileSystem.Events;
using System.Collections.Generic;
using System.Threading;

namespace Multitool.FileSystem
{
    public interface IFileSystemManager : IProgressNotifier
    {
        double CacheTimeout { get; set; }

        /// <summary>
        /// Fired when the one or more items in the cache have changed.
        /// </summary>
        event ItemChangedEventHandler Change;

        /// <summary>
        /// <para>
        /// List the content of a directory as a <see cref="IList{T}"/>.
        /// </para>
        /// <para>
        /// Because each directory size is calculated, the task can be 
        /// cancelled with the <paramref name="cancellationToken"/>.</para>
        /// </summary>
        /// <typeparam name="ItemType">Generic param of the <see cref="IList{T}"/></typeparam>
        /// <param name="path">System file path</param>
        /// <param name="cancellationToken">Cancellation token to cancel this method</param>
        /// <param name="list">Collection to add items to</param>
        /// <param name="addDelegate">Delegate to add items to the <paramref name="list"/></param>
        /// <exception cref="System.ArgumentNullException">
        /// If either <paramref name="list"/> or <paramref name="cancellationToken"/> is <see cref="null"/>
        /// </exception>
        void GetFileSystemEntries<ItemType>(string path, CancellationToken cancellationToken, ref IList<ItemType> list, AddDelegate<ItemType> addDelegate) where ItemType : IFileSystemEntry;

        /// <summary>
        /// Get the case sensitive path for the <paramref name="path"/> parameter.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        string GetRealPath(string path);

        /// <summary>
        /// Cleans the internal cache.
        /// </summary>
        void Reset();
    }

    public delegate void ItemChangedEventHandler(object sender, ChangeEventArgs data);
}