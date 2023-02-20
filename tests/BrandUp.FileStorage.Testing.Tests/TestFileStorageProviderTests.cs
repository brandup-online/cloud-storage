using BrandUp.FileStorage.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage.Testing.Tests
{
    public class TestFileStorageProviderTests : FileStorageTestBase
    {
        public TestFileStorageProviderTests() { }

        protected override void OnConfigure(IServiceCollection services, IFileStorageBuilder builder)
        {
            builder.AddTestProvider("memory", options =>
                {
                    options.MaxFileSize = 1024 * 1024 * 10;
                });

            services.AddFileContext<TestFileContext>("memory");
        }
    }
}