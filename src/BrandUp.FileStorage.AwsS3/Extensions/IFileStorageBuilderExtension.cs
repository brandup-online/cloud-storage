using BrandUp.FileStorage.Abstract;
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
        public static IFileStorageBuilder AddAwsS3Storage(this IFileStorageBuilder builder, Action<AwsS3Configuration> configureAction)
        {
            AwsS3Configuration options = new();
            configureAction(options);

            builder.AddStorage(typeof(AwsS3FileStorage<>), options);

            builder.Services.AddScoped(typeof(IMetadataSerializer<>), typeof(MetadataSerializer<>));

            return builder;
        }

        /// <summary>
        /// Adds Amazon S3 cloud to storage builder
        /// </summary>
        public static IFileStorageBuilder AddAwsS3Storage(this IFileStorageBuilder builder, IConfiguration configuration)
        {
            AwsS3Configuration options = new();
            configuration.GetSection("Default").Bind(options);

            options.InnerConfiguration = new Dictionary<string, AwsS3Configuration>();
            foreach (var config in configuration.GetChildren())
            {
                if (config.Key != "Default")
                {
                    AwsS3Configuration inner = new();
                    config.Bind(inner);
                    options.InnerConfiguration.Add(config.Key, inner);
                }
            }

            builder.AddStorage(typeof(AwsS3FileStorage<>), options);

            builder.Services.AddScoped(typeof(IMetadataSerializer<>), typeof(MetadataSerializer<>));

            return builder;
        }

        /// <summary>
        /// Adds bucket to storage builder
        /// </summary>
        /// <typeparam name="TFile">metadata for files in this bucket</typeparam>
        public static IFileStorageBuilder AddAwsS3Bucket<TFile>(this IFileStorageBuilder builder, string configKey = "") where TFile : class, new()
        {
            builder.AddFileToStorage<TFile>(typeof(AwsS3FileStorage<>), configKey);

            return builder;
        }

        /// <summary>
        /// Adds bucket to storage builder
        /// </summary>
        /// <typeparam name="TFile">metadata for files in this bucket</typeparam>
        public static IFileStorageBuilder AddAwsS3Bucket<TFile>(this IFileStorageBuilder builder, Action<AwsS3Configuration> configureAction) where TFile : class, new()
        {
            AwsS3Configuration options = new();
            configureAction(options);

            builder.AddFileToStorage<TFile>(typeof(AwsS3FileStorage<>), options);

            return builder;
        }
    }
}