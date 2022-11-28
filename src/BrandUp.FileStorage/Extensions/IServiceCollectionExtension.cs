using BrandUp.FileStorage.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage
{
    public static class IServiceCollectionExtension
    {
        public static FileStorageBuilder AddFileStorage(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return new FileStorageBuilder(services);
        }
    }
}
