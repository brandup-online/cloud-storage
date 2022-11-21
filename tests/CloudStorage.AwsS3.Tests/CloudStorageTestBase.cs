using BrandUp.CloudStorage.AwsS3.Extensions;
using BrandUp.CloudStorage.AwsS3.Tests._fakes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.CloudStorage.AwsS3.Tests
{
    public abstract class CloudStorageTestBase : IAsyncLifetime
    {
        readonly ServiceProvider rootServiceProvider;
        readonly IServiceScope serviceScope;

        public IServiceProvider RootServices => rootServiceProvider;
        public IServiceProvider Services => serviceScope.ServiceProvider;

        public CloudStorageTestBase()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var config = new ConfigurationBuilder()
               .AddJsonFile("appsettings.test.json")
               .Build();

            services.AddAwsS3Builder()
                .AddAwsCloudStorage(config.GetSection("TestStorage"))
                .AddClient<FakeFile>();

            OnConfigure(services);

            rootServiceProvider = services.BuildServiceProvider();
            serviceScope = rootServiceProvider.CreateScope();
        }


        #region IAsyncLifetime region

        public async Task InitializeAsync()
        {
            await OnInitializeAsync(rootServiceProvider, serviceScope.ServiceProvider);
        }

        public async Task DisposeAsync()
        {
            await OnFinishAsync(rootServiceProvider, serviceScope.ServiceProvider);

            serviceScope.Dispose();
            await rootServiceProvider.DisposeAsync();
        }

        #endregion

        #region Virtual members

        protected virtual void OnConfigure(IServiceCollection services) { }
        protected virtual Task OnInitializeAsync(IServiceProvider rootServices, IServiceProvider scopeServices) => Task.CompletedTask;
        protected virtual Task OnFinishAsync(IServiceProvider rootServices, IServiceProvider scopeServices) => Task.CompletedTask;

        #endregion
    }
}
