using BrandUp.FileStorage.Builder;
using BrandUp.MongoDB;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage.MongoDb
{
    public class MongoStorageProviderTests : FileStorageTestBase
    {
        readonly TestFileContext testFileContext;

        public MongoStorageProviderTests()
        {
            testFileContext = Services.GetRequiredService<TestFileContext>();
        }

        #region FileStorageTests members
        protected override void OnConfigure(IServiceCollection services, IFileStorageBuilder builder)
        {

            services.AddMongoDbContext<TestMongoContext>(builder =>
            {
                builder.DatabaseName = "Test";
                builder.UseCamelCaseElementName();
                builder.UseIgnoreIfNull(true);
            });

            services.AddMongo2GoDbClientFactory();

            builder.AddMongoProvider<TestMongoContext>("mongo", options => { });

            services.AddFileContext<TestFileContext>("mongo");
        }

        #endregion

        [Fact]
        public async Task Succsess_CRUD()
        {
            #region Preparation 

            var collection = testFileContext.FileStorageTestFiles;

            TestFile file = new()
            {
                FileName = "Test",
                Size = 100,
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow.Date,
            };

            using MemoryStream stream = new(image);

            #endregion

            await CRUD(collection, file, stream);
        }
    }

}