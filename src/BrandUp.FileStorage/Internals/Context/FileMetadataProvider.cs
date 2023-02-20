using System.ComponentModel;
using System.Reflection;

namespace BrandUp.FileStorage.Internals.Context
{
    internal class FileMetadataProvider
    {
        List<MetadataProperty> metadataProperties;

        public Type Type { get; init; }

        private FileMetadataProvider() { }

        public static FileMetadataProvider Create(Type metadataType)
        {
            var metadataProperties = new List<MetadataProperty>();
            var properties = metadataType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);
            foreach (var property in properties)
                metadataProperties.Add(new MetadataProperty(property));

            var metadataProvider = new FileMetadataProvider
            {
                Type = metadataType,
                metadataProperties = metadataProperties
            };

            return metadataProvider;
        }

        public TMetadata Deserialize<TMetadata>(IDictionary<string, string> input)
            where TMetadata : class, new()
        {
            var metadata = new TMetadata();

            foreach (var property in metadataProperties)
            {
                if (input.TryGetValue(property.Name, out var value))
                    property.SetValue(metadata, value);
            }

            return metadata;
        }

        public void Serialize<TMetadata>(TMetadata metadata, IDictionary<string, string> output)
        {
            foreach (var property in metadataProperties)
                output.Add(property.Name, property.GetValue(metadata));
        }
    }

    class MetadataProperty
    {
        readonly PropertyInfo property;
        readonly string name;

        public string Name => name;

        public MetadataProperty(PropertyInfo property)
        {
            this.property = property;

            name = property.Name;
        }

        public void SetValue(object metadataObject, string value)
        {
            var converter = TypeDescriptor.GetConverter(property);

            property.SetValue(metadataObject, converter.ConvertFromString(value));
        }

        public string GetValue(object metadataObject)
        {
            var converter = TypeDescriptor.GetConverter(property);

            return converter.ConvertToString(property.GetValue(metadataObject, null));
        }
    }
}

