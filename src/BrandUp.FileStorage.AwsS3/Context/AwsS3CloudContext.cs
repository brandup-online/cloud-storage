using BrandUp.FileStorage.AwsS3.Configuration;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace BrandUp.FileStorage.AwsS3.Context
{
    internal class AwsS3CloudContext
    {
        readonly IDictionary<Type, AwsS3Configuration> bucketConfigs = new Dictionary<Type, AwsS3Configuration>();
        readonly IDictionary<Type, PropertyInfo[]> typeMetaData = new Dictionary<Type, PropertyInfo[]>();
        readonly IList<Type> bucketTypes = new List<Type>();
        readonly IConfiguration configuration;

        internal AwsS3CloudContext(IConfiguration configuration)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        #region IAwsS3CloudContext members

        public IDictionary<Type, AwsS3Configuration> BucketConfigs => bucketConfigs;

        public IDictionary<Type, PropertyInfo[]> TypeMetaData => typeMetaData;

        public IList<Type> BucketTypes => bucketTypes;

        public void AddClientType<T>() where T : class
        {
            bucketTypes.Add(typeof(T));
            var properties = typeof(T).GetProperties();
            typeMetaData.Add(typeof(T), properties.ToArray());

            var options = configuration.GetSection(typeof(T).Name).Get<AwsS3Configuration>();
            if (options != null)
            {
                if (!bucketConfigs.TryAdd(typeof(T), options))
                    throw new ArgumentException($"Для типа {typeof(T)} уже сущесвует конфигурация");
            }
        }

        #endregion
    }
}