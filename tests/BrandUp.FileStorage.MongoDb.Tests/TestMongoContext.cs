using BrandUp.MongoDB;

namespace BrandUp.FileStorage.MongoDb.Tests
{

    public class TestMongoContext : MongoDbContext
    {
        public TestMongoContext(MongoDbContextOptions options) : base(options)
        {
        }
    }
}
