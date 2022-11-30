using Amazon.S3.Model;
using BrandUp.FileStorage.Builder;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace BrandUp.FileStorage.AwsS3
{
    public class MetadataSerializer<TMetadata> : IMetadataSerializer<TMetadata> where TMetadata : class, new()
    {
        readonly PropertyCacheCollection metadataProperties;

        readonly static Regex r = new("(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z0-9])", RegexOptions.Compiled);
        const string metadataKey = "X-Amz-Meta";

        public MetadataSerializer(IFileStorageBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (!builder.Properties.TryGetValue(typeof(TMetadata), out var props))
                throw new ArgumentException(typeof(TMetadata).Name);

            metadataProperties = props;
        }

        public FileInfo<TMetadata> Deserialize(Guid fileId, GetObjectMetadataResponse response)
        {
            var fileMetadata = new TMetadata();

            foreach (var property in metadataProperties)
            {
                var converter = TypeDescriptor.GetConverter(property.Property.PropertyType);

                var key = metadataKey + "-" + string.Join("-", ToOnlyFirstIsUpper(property.FullPropertyName.Split("."))); ;
                if (property.Property.PropertyType == typeof(string))
                    SetPropertyValue(fileMetadata, property.FullPropertyName, DecodeFileName(response.Metadata[key]));
                else
                    SetPropertyValue(fileMetadata, property.FullPropertyName, converter.ConvertFrom(response.Metadata[key]));
            }

            return new FileInfo<TMetadata> { Metadata = fileMetadata, Size = response.ContentLength, FileId = fileId };
        }

        public Dictionary<string, string> Serialize(TMetadata fileInfo)
        {
            var metadata = new Dictionary<string, string>();

            foreach (var property in metadataProperties)
            {
                var converter = TypeDescriptor.GetConverter(property.Property.PropertyType);
                if (property.Property.PropertyType == typeof(string))
                    metadata.Add(metadataKey + "-" + property.FullPropertyName.Replace(".", "-"), EncodeFileName(converter.ConvertToString(GetPropertyValue(fileInfo, property.FullPropertyName))));
                else
                    metadata.Add(metadataKey + "-" + property.FullPropertyName.Replace(".", "-"), converter.ConvertToString(GetPropertyValue(fileInfo, property.FullPropertyName)));
            }

            return metadata;
        }

        #region Helpers

        static object GetPropertyValue(object src, string propName)
        {
            if (src == null) throw new ArgumentException("Value cannot be null.", "src");
            if (propName == null) throw new ArgumentException("Value cannot be null.", "propName");

            if (propName.Contains("."))//complex type nested
            {
                var temp = propName.Split(new char[] { '.' }, 2);
                return GetPropertyValue(GetPropertyValue(src, temp[0]), temp[1]);
            }
            else
            {
                var prop = src.GetType().GetProperty(propName);
                return prop != null ? prop.GetValue(src, null) : null;
            }
        }

        static void SetPropertyValue(object src, string propName, object value)
        {
            if (src == null) throw new ArgumentException("Value cannot be null.", "src");
            if (propName == null) throw new ArgumentException("Value cannot be null.", "propName");

            if (propName.Contains(".")) // complex type nested
            {
                var temp = propName.Split(new char[] { '.' }, 2);
                var prop = src.GetType().GetProperty(temp[0]);

                var nestedObj = prop.GetValue(src, null);
                if (nestedObj == null)
                {
                    nestedObj = FormatterServices.GetUninitializedObject(prop.PropertyType);
                    prop.SetValue(src, nestedObj);
                }

                SetPropertyValue(nestedObj, temp[1], value);
            }
            else
            {
                var prop = src.GetType().GetProperty(propName);
                prop.SetValue(src, value);
            }
        }

        static string EncodeFileName(string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));

            return Convert.ToHexString(System.Text.Encoding.UTF8.GetBytes(fileName));
        }

        static string DecodeFileName(string encodedValue)
        {
            if (encodedValue == null)
                throw new ArgumentNullException(nameof(encodedValue));

            return System.Text.Encoding.UTF8.GetString(Convert.FromHexString(encodedValue));
        }

        static string[] ToOnlyFirstIsUpper(string[] values)
        {
            List<string> result = new();
            foreach (var value in values)
                result.Add(value[0] + value[1..].ToLowerInvariant());

            return result.ToArray();
        }

        #endregion
    }

    public interface IMetadataSerializer<TMetadata> where TMetadata : class, new()
    {
        Dictionary<string, string> Serialize(TMetadata fileInfo);
        FileInfo<TMetadata> Deserialize(Guid fileId, GetObjectMetadataResponse response);
    }
}
