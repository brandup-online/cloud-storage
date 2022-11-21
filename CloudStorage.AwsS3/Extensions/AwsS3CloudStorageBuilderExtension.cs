using Microsoft.Extensions.DependencyInjection;

namespace CloudStorage.AwsS3.Extensions
{
    public static class IServiceCollectionExtension
    {
        public static AwsS3CloudStorageBuilder AddAwsS3Builder(this IServiceCollection services)
        {
            return new AwsS3CloudStorageBuilder(services);
        }
    }
}
