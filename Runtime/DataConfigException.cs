using System;

namespace Dreamy.DataConfig
{
    public class DataConfigException : Exception
    {
        public DataConfigException(string documentName, string message)
            : base($"Data config '{documentName}': {message}")
        {
            DocumentName = documentName;
        }

        public DataConfigException(
            string documentName,
            string message,
            Exception innerException)
            : base($"Data config '{documentName}': {message}", innerException)
        {
            DocumentName = documentName;
        }

        public string DocumentName { get; }
    }
}
