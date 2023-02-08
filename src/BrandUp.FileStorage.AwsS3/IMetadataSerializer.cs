using Amazon.S3.Model;
using BrandUp.FileStorage.Abstract;
using BrandUp.FileStorage.Attributes;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace BrandUp.FileStorage.AwsS3
{
    /// <summary>
    /// Serializer to AWS metadata properties.
    /// </summary>
    /// <typeparam name="TMetadata">Metadata type of file.</typeparam>
    public class MetadataSerializer<TMetadata> : IMetadataSerializer<TMetadata> where TMetadata : class, IFileMetadata, new()
    {
        const string MetadataPrefix = "X-Amz-Meta";
        readonly IEnumerable<IPropertyCache> metadataProperties;

        public MetadataSerializer(IFileDefinitionsDictionary fileDefinitions)
        {
            if (fileDefinitions == null)
                throw new ArgumentNullException(nameof(fileDefinitions));

            if (!fileDefinitions.TryGetProperties(typeof(TMetadata), out metadataProperties))
                throw new ArgumentException(typeof(TMetadata).Name);
        }

        /// <summary>
        /// Serialize file metadata to Amazon S3 metadata.
        /// </summary>
        /// <returns>Dictioanary where key is amazon metadata key, and value is converted to string value of matadata</returns>
        public Dictionary<string, string> Serialize(TMetadata fileInfo)
        {
            var metadata = new Dictionary<string, string>();

            foreach (var property in metadataProperties)
            {
                var converter = TypeDescriptor.GetConverter(property.Property.PropertyType);
                var value = converter.ConvertToString(GetPropertyValue(fileInfo, property.FullPropertyName));
                var key = ToTrainCase(property.FullPropertyName.Replace(".", ""));

                if (property.Property.PropertyType == typeof(string))
                    metadata.Add(MetadataPrefix + "-" + key, EncodePropertyValue(value));
                else
                    metadata.Add(MetadataPrefix + "-" + key, value);
            }

            return metadata;
        }

        /// <summary>
        /// Deserialize Amazon S3 metadata to file metadata.
        /// </summary>
        public TMetadata Deserialize(Guid fileId, GetObjectMetadataResponse response)
        {
            var fileMetadata = new TMetadata();

            foreach (var property in metadataProperties)
            {
                var converter = TypeDescriptor.GetConverter(property.Property.PropertyType);

                var metadataKey = MetadataPrefix + "-" + string.Join("-", ToTrainCase(property.FullPropertyName.Replace(".", "")));
                var value = response.Metadata[metadataKey];
                if (value != null)
                {
                    if (property.Property.PropertyType == typeof(string))
                        SetPropertyValue(fileMetadata, property.FullPropertyName, DecodePropertyValue(value));
                    else
                        SetPropertyValue(fileMetadata, property.FullPropertyName, converter.ConvertFrom(value));
                }
            }

            return fileMetadata;
        }

        #region Helpers

        static object GetPropertyValue(object src, string propName)
        {
            if (src == null) throw new ArgumentNullException(nameof(src));
            if (propName == null) throw new ArgumentNullException(nameof(propName));

            if (propName.Contains('.')) // complex type nested
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
            if (src == null) throw new ArgumentNullException(nameof(src));
            if (propName == null) throw new ArgumentNullException(nameof(propName));

            if (propName.Contains('.')) // complex type nested
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

        static string EncodePropertyValue(string fileName)
        {
            return Convert.ToHexString(Encoding.UTF8.GetBytes(fileName));
        }

        static string DecodePropertyValue(string encodedValue)
        {
            return Encoding.UTF8.GetString(Convert.FromHexString(encodedValue));
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

                    if (attr.Name == propName)
                        return type.GetProperty(property.Name);
                }

                throw new ArgumentException(nameof(propName));
            }
        }

        static string ToTrainCase(string str)
        {
            if (str is null) return null;

            if (str.Length == 0) return string.Empty;

            StringBuilder builder = new();

            for (var i = 0; i < str.Length; i++)
            {
                if (i == 0) // if current char is the first char 
                {
                    builder.Append(char.ToUpper(str[i]));
                }
                else if (str[i] == '-')
                    continue;
                else if (char.IsLower(str[i])) // if current char is already lowercase
                {
                    builder.Append(str[i]);
                }
                else if (char.IsDigit(str[i]) && !char.IsDigit(str[i - 1])) // if current char is a number and the previous is not
                {
                    builder.Append('-');
                    builder.Append(str[i]);
                }
                else if (char.IsDigit(str[i])) // if current char is a number and previous is
                {
                    builder.Append(str[i]);
                }
                else if (char.IsLower(str[i - 1])) // if current char is upper and previous char is lower
                {
                    builder.Append('-');
                    builder.Append(str[i]);
                }
                else if (i + 1 == str.Length || char.IsUpper(str[i + 1])) // if current char is upper and next char doesn't exist or is upper
                {
                    builder.Append(char.ToLower(str[i]));
                }
                else // if current char is upper and next char is lower
                {
                    builder.Append('-');
                    builder.Append(str[i]);
                }
            }
            return builder.ToString();
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
