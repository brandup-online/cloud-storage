using BrandUp.FileStorage.Abstract;
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
        /// <param name="builder"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IFileStorageBuilder AddFolderStorage(this IFileStorageBuilder builder, IConfiguration configuration)
        {
            FolderConfiguration options = new();
            configuration.GetSection("Default").Bind(options);

            options.InnerConfiguration = new Dictionary<string, FolderConfiguration>();
            foreach (var config in configuration.GetChildren())
            {
                if (config.Key != "Default")
                {
                    FolderConfiguration inner = new();
                    config.Bind(inner);
                    options.InnerConfiguration.Add(config.Key, inner);
                }
            }

            builder.AddStorage(typeof(LocalFileStorage<>), options);

            return builder;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TFile"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IFileStorageBuilder AddFolderFor<TFile>(this IFileStorageBuilder builder) where TFile : class, new()
        {
            builder.AddFileToStorage<TFile>(typeof(LocalFileStorage<>));

            return builder;
        }
    }
}