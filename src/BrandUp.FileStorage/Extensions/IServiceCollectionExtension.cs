using BrandUp.FileStorage.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage
{
    /// <summary>
    /// Extension of IServiceCollection for creating FileStorageBuilder
    /// </summary>
    public static class IServiceCollectionExtension
    {
        /// <summary>
        /// Adds builder to DI
        /// </summary>
        /// <param name="services"></param>
        /// <returns>Builder</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static FileStorageBuilder AddFileStorage(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return new FileStorageBuilder(services);
        }
    }
}
