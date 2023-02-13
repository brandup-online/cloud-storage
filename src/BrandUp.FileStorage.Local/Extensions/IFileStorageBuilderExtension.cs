using BrandUp.FileStorage.Abstract;
using BrandUp.FileStorage.Abstract.Configuration;
using BrandUp.FileStorage.FileSystem.Configuration;
using Microsoft.Extensions.Configuration;

namespace BrandUp.FileStorage.FileSystem
{
    /// <summary>
    /// 
    /// </summary>
    public static class IFileStorageBuilderExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="defaultConfiguration"></param>
        /// <returns></returns>
        public static IFileStorageBuilder AddFolderStorage(this IFileStorageBuilder builder, Action<FolderConfiguration> defaultConfiguration)
        {
            FolderConfiguration options = new();
            defaultConfiguration(options);
            builder.AddStorage(typeof(FileSystemStorage<>), new Dictionary<string, IStorageConfiguration> { { "Default", options } });

            return builder;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IFileStorageBuilder AddFolderStorage(this IFileStorageBuilder builder, IConfiguration configuration)
        {
            builder.AddStorageWithConfiguration(typeof(FileSystemStorage<>), typeof(FolderConfiguration), configuration);

            return builder;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TFile"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IFileStorageBuilder AddFolderFor<TFile>(this IFileStorageBuilder builder) where TFile : class, IFileMetadata, new()
        {
            builder.AddFileToStorage<TFile>(typeof(FileSystemStorage<>), null);

            return builder;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TFile"></typeparam>
        /// <param name="builder"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static IFileStorageBuilder AddFolderFor<TFile>(this IFileStorageBuilder builder, string key) where TFile : class, IFileMetadata, new()
        {
            builder.AddFileToStorage<TFile>(typeof(FileSystemStorage<>), null, key);

            return builder;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TFile"></typeparam>
        /// <param name="builder"></param>
        /// <param name="configurationAction"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static IFileStorageBuilder AddFolderFor<TFile>(this IFileStorageBuilder builder, Action<FolderConfiguration> configurationAction, string key = "") where TFile : class, IFileMetadata, new()
        {
            FolderConfiguration options = new();
            configurationAction(options);

            builder.AddFileToStorage<TFile>(typeof(FileSystemStorage<>), options, key);

            return builder;
        }
    }
}