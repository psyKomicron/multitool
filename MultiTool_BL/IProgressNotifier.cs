﻿using Multitool.FileSystem.Events;

namespace Multitool
{
    public interface IProgressNotifier
    {
        /// <summary>
        /// Fires each time a subtask is completed.
        /// </summary>
        event TaskProgressEventHandler Progress;
        /// <summary>
        /// Fired when the task fails. Carries the exception that caused the failure.
        /// </summary>
        event TaskFailedEventHandler Exception;

        event TaskCompletedEventHandler Completed;

        /// <summary>
        /// Set it to true to allow to fire the <see cref="Progress"/> event.
        /// </summary>
        bool Notify { get; set; }
    }
}
