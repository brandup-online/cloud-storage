using BrandUp.FileStorage.Abstract;
using BrandUp.FileStorage.AwsS3;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage.Tests.Storages.Aws
{
    public class FakeFile : IFileMetadata
    {
        public string FileName { get; set; }
        public string Extension { get; set; }
        public int FakeInt { get; set; }
        public DateTime FakeDateTime { get; set; }
        public TimeSpan FakeTimeSpan { get; set; }
        public FakeInnnerClass FakeInner { get; set; }
        public class FakeInnnerClass
        {
            public bool FakeBool { get; set; }
            public Guid FakeGuid { get; set; }
        }
    }

    public class AwsClientTest : FileStorageTestBase<FakeFile>
    {
        #region FileStorageTestBase

        protected override FakeFile CreateMetadataValue()
        {
            return new FakeFile
            {
                FakeInner = new() { FakeGuid = Guid.NewGuid(), FakeBool = true },
                FileName = "string",
                Extension = "png",
                FakeDateTime = new DateTime(2002, 12, 15, 13, 45, 0),
                FakeInt = 21332,
                FakeTimeSpan = TimeSpan.FromSeconds(127)
            };
        }

        protected override void OnConfigure(IServiceCollection services, IFileStorageBuilder builder)
        {
            builder.AddAwsS3Storage(Configuration.GetSection("TestCloudStorage"))
                        .AddAwsS3Bucket<FakeFile>("FakeAwsFile");
            //.AddAwsS3Bucket<_fakes.AttributedFakeFile>("FakeAwsFile")
            //.AddAwsS3Bucket<_fakes.FakeMetadataOld>("FakeAwsFile")
            //.AddAwsS3Bucket<_fakes.FakeMetadataNew>("FakeAwsFile")
            //.AddAwsS3Bucket<_fakes.FakeRequiredFile>("FakeAwsFile");
            base.OnConfigure(services, builder);
        }

        #endregion
    }
}
