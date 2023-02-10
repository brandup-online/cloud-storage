using BrandUp.FileStorage.Abstract;
using BrandUp.FileStorage.Abstract.Configuration;
using BrandUp.FileStorage.AwsS3.Configuration;
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
        public static IFileStorageBuilder AddAwsS3Storage(this IFileStorageBuilder builder, Dictionary<string, AwsS3Configuration> options)
        {
            builder.AddStorage(typeof(AwsS3FileStorage<>), options.ToDictionary(k => k.Key, v => (IStorageConfiguration)v.Value));

            builder.Services.AddScoped(typeof(IMetadataSerializer<>), typeof(MetadataSerializer<>));

            return builder;
        }

        /// <summary>
        /// Adds Amazon S3 cloud to storage builder
        /// </summary>
        public static IFileStorageBuilder AddAwsS3Storage(this IFileStorageBuilder builder, Action<AwsS3Configuration> defaultConfiguration)
        {
            AwsS3Configuration options = new();
            defaultConfiguration(options);
            builder.AddStorage(typeof(AwsS3FileStorage<>), new Dictionary<string, IStorageConfiguration> { { "Default", options } });

            builder.Services.AddScoped(typeof(IMetadataSerializer<>), typeof(MetadataSerializer<>));

            return builder;
        }

        /// <summary>
        /// Adds Amazon S3 cloud to storage builder
        /// </summary>
        public static IFileStorageBuilder AddAwsS3Storage(this IFileStorageBuilder builder, IConfiguration configuration)
        {
            builder.AddStorageWithConfiguration(typeof(AwsS3FileStorage<>), typeof(AwsS3Configuration), configuration);

            builder.Services.AddScoped(typeof(IMetadataSerializer<>), typeof(MetadataSerializer<>));

            return builder;
        }

        /// <summary>
        /// Adds bucket to storage builder
        /// </summary>
        /// <typeparam name="TFile">metadata for files in this bucket</typeparam>
        public static IFileStorageBuilder AddAwsS3Bucket<TFile>(this IFileStorageBuilder builder, string configKey = "") where TFile : class, IFileMetadata, new()
        {
            builder.AddFileToStorage<TFile>(typeof(AwsS3FileStorage<>), null, configKey);

            return builder;
        }

        /// <summary>
        /// Adds bucket to storage builder
        /// </summary>
        /// <typeparam name="TFile">metadata for files in this bucket</typeparam>
        public static IFileStorageBuilder AddAwsS3Bucket<TFile>(this IFileStorageBuilder builder, Action<AwsS3Configuration> configureAction, string configKey = "") where TFile : class, IFileMetadata, new()
        {
            AwsS3Configuration options = new();
            configureAction(options);

            builder.AddFileToStorage<TFile>(typeof(AwsS3FileStorage<>), options, configKey);

            return builder;
        }
    }
}