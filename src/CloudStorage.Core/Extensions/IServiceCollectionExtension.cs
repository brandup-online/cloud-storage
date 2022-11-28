using BrandUp.CloudStorage.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.CloudStorage
{
    public static class IServiceCollectionExtension
    {
        public static CloudStorageBuilder AddCloudStorage(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return new CloudStorageBuilder(services);
        }
    }
}
