using BrandUp.FileStorage.Abstract;
using BrandUp.FileStorage.AwsS3;
using BrandUp.FileStorage.AwsS3.Configuration;
using BrandUp.FileStorage.Folder;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage.Builder
{
    public class ExtensionsTest : FileStorageTestBase
    {
        protected override void OnConfigure(IServiceCollection services, IFileStorageBuilder builder)
        {
            builder.AddAwsS3Storage(new Dictionary<string, AwsS3Configuration>()
            {
                {
                    "Default" ,
                    new()
                    {
                        ServiceUrl = "https://s3.yandexcloud.net",
                        AuthenticationRegion = "ru-central1",
                        AccessKeyId = "adadass",
                        SecretAccessKey = "adadass"
                    }
                },
                {
                    "FakeAwsFile" ,
                    new()
                    {
                        BucketName = "bucket"
                    }
                },
            })
            .AddAwsS3Bucket<Storages.Aws._fakes.FakeFile>("FakeAwsFile")
            .AddAwsS3Bucket<Storages.Aws._fakes.AttributedFakeFile>((options) =>
            {
                options.BucketName = "bucket2";
            }, "FakeAwsFile2");

            builder.AddFolderStorage((o) => { })
            .AddFolderFor<Storages.Local._fakes.FakeFile>((o) =>
            {
                o.MetadataPath = "D:\\Test";
                o.ContentPath = "D:\\Test\\Metadata";
            }, "FakeLocalFile");

            base.OnConfigure(services, builder);
        }

        [Fact]
        public void Success()
        {
            var awsFakeStorage = Services.GetService<IFileStorage<Storages.Aws._fakes.FakeFile>>();
            Assert.NotNull(awsFakeStorage);
            var awsFakeStorage2 = Services.GetService<IFileStorage<Storages.Aws._fakes.AttributedFakeFile>>();
            Assert.NotNull(awsFakeStorage2);
            var localFakeStorage = Services.GetService<IFileStorage<Storages.Local._fakes.FakeFile>>();
            Assert.NotNull(localFakeStorage);
        }
    }
}
