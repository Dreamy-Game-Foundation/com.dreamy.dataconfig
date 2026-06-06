namespace Dreamy.DataConfig
{
    public interface IDataConfigTable
    {
        int SchemaVersion { get; }

        void Initialize(string documentName);
    }
}
