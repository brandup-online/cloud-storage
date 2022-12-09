using BrandUp.FileStorage.Builder;
using BrandUp.FileStorage.Folder.Configuration;
using Microsoft.Extensions.Configuration;

namespace BrandUp.FileStorage.Folder
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
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IFileStorageBuilder AddFolderStorage(this IFileStorageBuilder builder, IConfiguration configuration)
        {
            FolderConfiguration config = new();

            configuration.Bind(config);
            builder.AddConfiguration(typeof(LocalFileStorage<>), config);

            return builder;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TFile"></typeparam>
        /// <param name="builder"></param>
        /// <param name="configureAction"></param>
        /// <returns></returns>
        public static IFileStorageBuilder AddFolderFor<TFile>(this IFileStorageBuilder builder, Action<FolderConfiguration> configureAction) where TFile : class, new()
        {
            var options = new FolderConfiguration();
            configureAction(options);

            builder.AddFileToStorage<TFile>(options);

            return builder;
        }
    }
}