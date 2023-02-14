using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BrandUp.FileStorage.Testing.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var service = new ServiceCollection();

            service
                .AddFileStorage()
                .AddTestProvider("memory", options =>
                {
                    options.MaxFileSize = 1024 * 1024 * 10;
                });

            service
                .AddFileContext<TestFileContext>("memory");

            var serviceProvider = service.BuildServiceProvider(true);

            using var scope = serviceProvider.CreateScope();
            var fileContext = scope.ServiceProvider.GetRequiredService<TestFileContext>();
            Assert.NotNull(fileContext);
            Assert.NotNull(fileContext.StorageProvider);

            var tempFiles = fileContext.TempFiles;
            Assert.NotNull(tempFiles);
        }

        public class TestFileContext : FileStorageContext
        {
            public IFileCollection<TestFile> TempFiles => GetCollection<TestFile>("temp-files");
        }

        public class TestFile
        {
            public string FileName { get; set; }
        }
    }
}