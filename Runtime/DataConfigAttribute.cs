using System;

namespace Dreamy.DataConfig
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class DataConfigAttribute : Attribute
    {
        public DataConfigAttribute(string documentName)
        {
            if (string.IsNullOrWhiteSpace(documentName))
            {
                throw new ArgumentException(
                    "Document name cannot be empty.",
                    nameof(documentName));
            }

            DocumentName = documentName;
        }

        public string DocumentName { get; }
    }
}
