using BrandUp.FileStorage.AwsS3.Configuration;
using BrandUp.FileStorage.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
        public static IFileStorageBuilder AddAwsS3Storage(this IFileStorageBuilder builder, IConfiguration configuration)
        {
            AwsS3Configuration config = new();

            configuration.Bind(config);
            builder.AddConfiguration(typeof(AwsS3FileStorage<>), config);

            builder.Services.AddScoped(typeof(IMetadataSerializer<>), typeof(MetadataSerializer<>));

            return builder;
        }

        /// <summary>
        /// Adds bucket to storage builder
        /// </summary>
        /// <typeparam name="TFile">metadata for files in this bucket</typeparam>
        public static IFileStorageBuilder AddAwsS3Bucket<TFile>(this IFileStorageBuilder builder, Action<AwsS3Configuration> configureAction) where TFile : class, new()
        {
            var options = new AwsS3Configuration();
            configureAction(options);

            builder.AddFileToStorage<TFile>(options);

            return builder;
        }
    }
}