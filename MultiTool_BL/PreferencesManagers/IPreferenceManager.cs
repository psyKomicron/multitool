namespace Multitool.PreferencesManagers
{
    public interface IPreferenceManager
    {
        string Path { get; set; }
        PreferenceManagerType Type { get; }

        /// <summary>
        /// Adds a <typeparamref name="DataType"/> object to the preference file. <paramref name="name"/> will be used to name
        /// the data so it can be retreived later by <see cref="GetWindowManager{DataType}(string)"/>.
        /// </summary>
        /// <typeparam name="DataType">Type of <paramref name="data"/>.</typeparam>
        /// <param name="data">Data to add to the preference file.</param>
        /// <param name="name">Key parameter of the <see cref="GetWindowManager{DataType}(string)"/> method.</param>
        void AddWindowManager<DataType>(DataType data, string name) where DataType : class;
        void DeserializePreferenceManager();
        /// <summary>
        /// Load the data for the serialized item with the key <paramref name="key"/>.
        /// The data will be load into an instance of <typeparamref name="DataType"/>.
        /// </summary>
        /// <typeparam name="DataType">The type of DTO to hold the data.</typeparam>
        /// <param name="key">Key for the data.</param>
        /// <returns></returns>
        DataType GetWindowManager<DataType>(string key) where DataType : class, new();
        /// <summary>
        /// Save the informations stored in the <see cref="IPreferenceManager{WindowManagerType}"/>
        /// to a file (the file format is determined by implementation).
        /// </summary>
        void SerializePreferenceManager();
        //bool CleanSerialization();
    }
}