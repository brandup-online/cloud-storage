using BrandUp.FileStorage.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage
{
    public abstract class FileStorageTestBase : IAsyncLifetime
    {
        public static Guid TestGuid;

        readonly ServiceProvider rootServiceProvider;
        readonly IServiceScope serviceScope;
        readonly IConfiguration configuration;

        public IServiceProvider RootServices => rootServiceProvider;
        public IServiceProvider Services => serviceScope.ServiceProvider;
        public IConfiguration Configuration => configuration;

        public FileStorageTestBase()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var configBuilder = new ConfigurationBuilder()
              .AddUserSecrets(typeof(FileStorageTestBase).Assembly)
              .AddJsonFile("appsettings.test.json", true)
              .AddEnvironmentVariables();

            OnConfigurationBuilding(configBuilder);

            configuration = configBuilder.Build();

            var builder = services.AddFileStorage();

            OnConfigure(services, builder);

            rootServiceProvider = services.BuildServiceProvider();
            serviceScope = rootServiceProvider.CreateScope();
        }

        static FileStorageTestBase()
        {
            TestGuid = Guid.Parse("ea3dd067-8bd8-4792-bfb0-e7be02d912e1");
        }


        #region Virtual members

        protected virtual void OnConfigurationBuilding(IConfigurationBuilder builder) { }
        protected virtual void OnConfigure(IServiceCollection services, IFileStorageBuilder builder) { }
        protected virtual Task OnInitializeAsync(IServiceProvider rootServices, IServiceProvider scopeServices) => Task.CompletedTask;
        protected virtual Task OnFinishAsync(IServiceProvider rootServices, IServiceProvider scopeServices) => Task.CompletedTask;

        #endregion

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
    }
}
