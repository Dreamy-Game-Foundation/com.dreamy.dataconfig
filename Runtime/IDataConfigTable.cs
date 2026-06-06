namespace Dreamy.DataConfig
{
    public interface IDataConfigTable
    {
        string DocumentName { get; }

        int SchemaVersion { get; }

        void Initialize(string documentName);
    }
}
