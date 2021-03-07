namespace MultiTool.Windows
{
    public interface ISerializableWindow
    {
        /// <summary>
        /// Called just before application shutdown to "serialize" the data stored with the window.
        /// </summary>
        void Serialize();
        /// <summary>
        /// Called after window instanciation to "deserialize" the data stored with the window.
        /// </summary>
        void Deserialize();
    }
}
