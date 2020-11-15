namespace BusinessLayer.PreferencesManager
{
    public interface IPreferenceManager<WindowManagerType> where WindowManagerType : IWindowPreferenceManager
    {
        string Path { get; set; }

        void SerializePreferenceManager();
        void DeserializePreferenceManager();
        WindowManagerType GetWindowManager(string name);
        WindowManagerType AddPreferenceManager(WindowManagerType manager);
    }
}