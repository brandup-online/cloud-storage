using BrandUp.FileStorage.Abstract;
using BrandUp.FileStorage.Tests._fakes.Aws;
using BrandUp.FileStorage.Tests._fakes.Local;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage.Tests
{
    public class FactoryTest : FileStorageTestBase
    {

        readonly IFileStorageFactory factory;

        public FactoryTest()
        {
            factory = Services.GetRequiredService<IFileStorageFactory>();
        }

        /// <summary>
        /// Factory in scope return one instance for each storage
        /// </summary>
        [Fact]
        public void SuccsesSameInstanses()
        {
            using var awsStorage1 = factory.Create<FakeAwsFile>();
            using var localStorage1 = factory.Create<FakeLocalFile>();
            using var awsStorage2 = factory.Create<FakeAwsFile>();
            using var localStorage2 = factory.Create<FakeLocalFile>();

            Assert.Same(awsStorage1, awsStorage2);
            Assert.Same(localStorage1, localStorage2);
        }
    }
}
