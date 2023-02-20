using BrandUp.FileStorage.AwsS3.Configuration;
using BrandUp.FileStorage.Builder;

namespace BrandUp.FileStorage.AwsS3
{
    /// <summary>
    /// 
    /// </summary>
    public static class IFileStorageBuilderExtension
    {
        /// <summary>
        /// Adds Amazon S3 cloud to storage builder
        /// </summary>
        public static IFileStorageBuilder AddAwsS3Storage(this IFileStorageBuilder builder, string configurationName, Action<AwsS3Configuration> options)
        {
            builder.AddStorageProvider<AwsS3FileStorageProvider, AwsS3Configuration>(configurationName, options);

            return builder;
        }
    }
}