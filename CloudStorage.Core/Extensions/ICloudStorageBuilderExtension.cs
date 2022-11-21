using CloudStorage.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace CloudStorage.Extensions
{
    public static class ICloudStorageBuilderExtension
    {
        public static ICloudStorageBuilder AddStorage<TStorage>(this ICloudStorageBuilder builder) where TStorage : class, ICloudStorage
        {
            builder.Services.AddSingleton<ICloudStorage, TStorage>();

            return builder;
        }
    }
}
