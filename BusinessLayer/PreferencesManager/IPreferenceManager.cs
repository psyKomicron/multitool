namespace BusinessLayer.PreferencesManagers
{
    public interface IPreferenceManager<WindowManagerType> where WindowManagerType : IWindowPreferenceManager
    {
        string Path { get; set; }

        /// <summary>
        /// Save the informations stored in the <see cref="IPreferenceManager{WindowManagerType}"/>
        /// to a file (the file format is determined by implementation).
        /// </summary>
        void SerializePreferenceManager();
        void DeserializePreferenceManager();
        WindowManagerType GetWindowManager(string name);
        WindowManagerType AddPreferenceManager(WindowManagerType manager);
    }
}