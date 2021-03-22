namespace BusinessLayer
{
    public interface IProgressNotifier
    {
        event ProgressEventHandler Progress;

        bool NotifyProgress { get; set; }
    }
}
