using BrandUp.FileStorage.Builder;
using BrandUp.FileStorage.FileSystem.Configuration;

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
        /// <param name="configurationName"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IFileStorageBuilder AddFolderStorage(this IFileStorageBuilder builder, string configurationName, Action<FolderConfiguration> options)
        {
            builder.AddStorageProvider<FileSystemStorageProvider, FolderConfiguration>(configurationName, options);

            return builder;
        }
    }
}