namespace BusinessLayer.PreferencesManagers
{
    public interface IPreferenceManager
    {
        string Path { get; set; }

        void AddWindowManager(WindowPreferenceManager manager);
        void DeserializePreferenceManager();
        DataType GetWindowManager<DataType>(string name) where DataType : class, new();
        /// <summary>
        /// Save the informations stored in the <see cref="IPreferenceManager{WindowManagerType}"/>
        /// to a file (the file format is determined by implementation).
        /// </summary>
        void SerializePreferenceManager();
    }
}