using BrandUp.FileStorage.Abstract;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage.Tests
{
    public abstract class FileStorageTest<T> : FileStorageTestBase<T> where T : class, IFileMetadata, new()
    {

        #region Asserts

        /// <summary>
        /// Scope returns only one instance of storage 
        /// </summary>
        [Fact]
        public void Succses_SameInstanses()
        {
            using var storage1 = Services.GetRequiredService<IFileStorage<T>>();
            using var storage2 = Services.GetRequiredService<IFileStorage<T>>();

            Assert.Same(storage1, storage2);
        }

        [Fact]
        public void Succses_DifferentScopes()
        {
            using var scope1 = Services.CreateScope();
            using var storage1 = scope1.ServiceProvider.GetRequiredService<IFileStorage<T>>();

            using var scope2 = Services.CreateScope();
            using var storage2 = scope2.ServiceProvider.GetRequiredService<IFileStorage<T>>();

            Assert.NotSame(storage1, storage2);
        }

        [Fact]
        public async Task Success_CRUD()
        {
            using var stream = new MemoryStream(image);
            var metadata = CreateMetadataValue();

            await DoCRUD(metadata, stream);
        }

        #endregion
    }
}
