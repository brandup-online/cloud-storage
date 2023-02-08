using BrandUp.FileStorage.Abstract;
using BrandUp.FileStorage.Folder.Configuration;

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
        /// <param name="configureAction"></param>
        /// <returns></returns>
        public static IFileStorageBuilder AddFolderStorage(this IFileStorageBuilder builder, Action<FolderConfiguration> configureAction)
        {
            var options = new FolderConfiguration();
            configureAction(options);

            builder.AddStorage(typeof(LocalFileStorage<>), options);

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

            builder.AddFileToStorage<TFile>(typeof(LocalFileStorage<>), options);

            return builder;
        }
    }
}