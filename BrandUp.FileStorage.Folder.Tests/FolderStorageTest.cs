using BrandUp.FileStorage.Folder.Tests._fakes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage.Folder.Tests
{
    public class FolderStorageTest : IAsyncLifetime
    {
        readonly IFileStorageFactory factory;

        readonly ServiceProvider rootServiceProvider;
        readonly IServiceScope serviceScope;

        public FolderStorageTest()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var config = new ConfigurationBuilder()
               .AddJsonFile("appsettings.test.json", true)
               .AddEnvironmentVariables()
               .Build();

            var builder = services.AddFileStorage();

            builder.AddFolderStorage(config.GetSection("TestFolderStorage:Default"))
                    .AddFolderFor<FakeFile>(o => config.GetSection("TestFolderStorage:FakeFile").Bind(o));

            rootServiceProvider = services.BuildServiceProvider();
            serviceScope = rootServiceProvider.CreateScope();

            factory = rootServiceProvider.GetRequiredService<IFileStorageFactory>();
        }

        #region IAsyncLifetime members

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            serviceScope.Dispose();
            await rootServiceProvider.DisposeAsync();
        }

        #endregion

        [Fact]
        public Task Success()
        {

        }
    }
}