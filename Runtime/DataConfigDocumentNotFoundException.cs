namespace Dreamy.DataConfig
{
    public sealed class DataConfigDocumentNotFoundException
        : DataConfigException
    {
        public DataConfigDocumentNotFoundException(
            string documentName,
            string message)
            : base(documentName, message)
        {
        }
    }
}
