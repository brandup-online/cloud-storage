using BrandUp.FileStorage.Attributes;
using BrandUp.FileStorage.Exceptions;
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
                if (input.TryGetValue(property.Name, out var value) && !property.IsIgnore)
                    property.SetValue(metadata, value);
            }

            return metadata;
        }

        public void Serialize<TMetadata>(TMetadata metadata, IDictionary<string, string> output)
        {
            foreach (var property in metadataProperties)
            {
                if (!property.IsIgnore)
                    output.Add(property.Name, property.GetValue(metadata));
            }
        }
    }

    class MetadataProperty
    {
        readonly PropertyInfo property;
        readonly string name;
        readonly bool isIgnore;
        readonly bool isRequired;

        public string Name => name;
        public bool IsIgnore => isIgnore;

        public MetadataProperty(PropertyInfo property)
        {
            this.property = property;

            name = GetPropertyName(property);

            var ignoreAttribute = Attribute.GetCustomAttribute(property, typeof(MetadataIgnoreAttribute));
            if (ignoreAttribute != null)
                isIgnore = true;
            else
                isIgnore = false;

            var requiredAttribute = Attribute.GetCustomAttribute(property, typeof(MetadataRequiredAttribute));
            if (requiredAttribute != null)
                isRequired = true;
            else
                isRequired = false;
        }

        public string GetValue(object metadataObject)
        {
            var converter = TypeDescriptor.GetConverter(property);

            var value = property.GetValue(metadataObject, null);
            if (value == null && isRequired)
                throw new PropertyRequiredException(nameof(value));

            return converter.ConvertToString(value);
        }

        public void SetValue(object metadataObject, string value)
        {
            var converter = TypeDescriptor.GetConverter(property.PropertyType);

            property.SetValue(metadataObject, converter.ConvertFromString(value));
        }

        string GetPropertyName(PropertyInfo property)
        {
            var propertyAttribute = (MetadataPropertyAttribute)Attribute.GetCustomAttribute(property, typeof(MetadataPropertyAttribute));
            if (propertyAttribute != null)
                return propertyAttribute.Name;

            return property.Name;
        }
    }
}


