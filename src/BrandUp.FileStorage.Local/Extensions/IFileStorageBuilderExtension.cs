using BrandUp.FileStorage.Builder;
using BrandUp.FileStorage.Folder.Configuration;
using Microsoft.Extensions.Configuration;

namespace BrandUp.FileStorage.Folder
{
    public static class IFileStorageBuilderExtension
    {
        public static IFileStorageBuilder AddFolderStorage(this IFileStorageBuilder builder, IConfiguration configuration)
        {
            FolderConfiguration config = new();

            configuration.Bind(config);
            builder.AddConfiguration(typeof(LocalFileStorage<>), config);

            return builder;
        }

        public static IFileStorageBuilder AddFolderFor<TFile>(this IFileStorageBuilder builder, Action<FolderConfiguration> configureAction) where TFile : class, new()
        {
            var options = new FolderConfiguration();
            configureAction(options);

            builder.AddFileToStorage<TFile>(options);

            return builder;
        }
    }
}