using BrandUp.FileStorage.Builder;

namespace BrandUp.FileStorage.Testing
{
    public static class IFileStorageBuilderExtension
    {
        public static IFileStorageBuilder AddTestProvider(this IFileStorageBuilder builder, string configurationName, Action<TestStorageOptions> configureOptions)
        {
            builder.AddStorageProvider<TestStorageProvider, TestStorageOptions>(configurationName, configureOptions);

            return builder;
        }
    }
}