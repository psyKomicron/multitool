namespace MultiTool.Windows
{
    public interface ISerializableWindow<DTOType> where DTOType : class, new()
    {
        DTOType Data { get; set; }

        void Serialize();

        void Deserialize();
    }
}
