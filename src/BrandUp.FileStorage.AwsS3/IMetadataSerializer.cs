using Amazon.S3.Model;
using BrandUp.FileStorage.Attributes;
using BrandUp.FileStorage.Builder;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;

namespace BrandUp.FileStorage.AwsS3
{
    /// <summary>
    /// Serializer to AWS metadata properties
    /// </summary>
    /// <typeparam name="TMetadata"></typeparam>
    public class MetadataSerializer<TMetadata> : IMetadataSerializer<TMetadata> where TMetadata : class, IFileMetadata, new()
    {
        readonly PropertyCacheCollection metadataProperties;

        const string metadataKey = "X-Amz-Meta";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="builder">Storage builer that stored properties info for metadata class</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public MetadataSerializer(IFileStorageBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (!builder.Properties.TryGetValue(typeof(TMetadata), out var props))
                throw new ArgumentException(typeof(TMetadata).Name);

            metadataProperties = props;
        }

        /// <summary>
        /// Deserialize Amazon S3 metadata to file metadata
        /// </summary>
        /// <param name="fileId">file</param>
        /// <param name="response"></param>
        /// <returns></returns>
        public TMetadata Deserialize(Guid fileId, GetObjectMetadataResponse response)
        {
            var fileMetadata = new TMetadata();

            foreach (var property in metadataProperties)
            {
                var converter = TypeDescriptor.GetConverter(property.Property.PropertyType);

                var key = metadataKey + "-" + string.Join("-", ToOnlyFirstIsUpper(property.FullPropertyName.Split(".")));
                var value = response.Metadata[key];
                if (value != null)
                {
                    if (property.Property.PropertyType == typeof(string))
                        SetPropertyValue(fileMetadata, property.FullPropertyName, DecodeFileName(value));
                    else
                        SetPropertyValue(fileMetadata, property.FullPropertyName, converter.ConvertFrom(value));
                }
            }

            return fileMetadata;
        }

        /// <summary>
        /// Serialize file metadata to Amazon S3 metadata
        /// </summary>
        /// <param name="fileInfo">file metadata </param>
        /// <returns>Dictioanary where key is amazon metadata key, and value is converted to string value of matadata</returns>
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
            if (src == null) throw new ArgumentException("Value cannot be null.", nameof(src));
            if (propName == null) throw new ArgumentException("Value cannot be null.", nameof(propName));

            if (propName.Contains("."))//complex type nested
            {
                var temp = propName.Split(new char[] { '.' }, 2);
                return GetPropertyValue(GetPropertyValue(src, temp[0]), temp[1]);
            }
            else
            {
                var prop = GetProperty(src, propName);
                return prop.GetValue(src, null);
            }
        }

        static void SetPropertyValue(object src, string propName, object value)
        {
            if (src == null) throw new ArgumentException("Value cannot be null.", nameof(src));
            if (propName == null) throw new ArgumentException("Value cannot be null.", nameof(propName));

            if (propName.Contains(".")) // complex type nested
            {
                var temp = propName.Split(new char[] { '.' }, 2);
                var prop = GetProperty(src, temp[0]);

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
                var prop = GetProperty(src, propName);
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

        static PropertyInfo GetProperty(object obj, string propName)
        {
            var type = obj.GetType();
            var prop = type.GetProperty(propName);
            if (prop != null)
                return prop;
            else
            {
                var properties = type.GetProperties();
                foreach (var property in properties)
                {
                    var attr = property.GetCustomAttribute<MetadataKeyAttribute>();

                    if (attr == null)
                        continue;

                    if (attr.MetadataKey == propName)
                        return type.GetProperty(property.Name);
                }

                throw new ArgumentException(nameof(propName));
            }
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TMetadata"></typeparam>
    public interface IMetadataSerializer<TMetadata> where TMetadata : class, IFileMetadata, new()
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        Dictionary<string, string> Serialize(TMetadata fileInfo);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        TMetadata Deserialize(Guid fileId, GetObjectMetadataResponse response);
    }
}
