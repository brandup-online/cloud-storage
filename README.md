# file-storage

# Using

[![Build status](https://dev.azure.com/brandup/BrandUp%20Core/_apis/build/status/BrandUp.CloudStorage)](https://dev.azure.com/brandup/BrandUp%20Core/_build/latest?definitionId=8)

Adding to DI
```
var builder = services.AddFileStorage();

//Example AwsS3Storage
builder.AddAwsS3Storage(config.GetSection("@Default configuration section@")) // For common configuration that will not changing in all Types
       .AddAwsS3Bucket<Type>(o => config.GetSection("@Spesific for Type section@").Bind(o)); // May be not full

//Example FolderStorage
builder.AddFolderStorage(config.GetSection("@Default configuration section@")) // For common configuration that will not changing in all Types
       .AddFolderFor<Type>(o => config.GetSection("@Spesific for Type section@").Bind(o)); // May be not full
```
Creating storage

```
public SomeClass(IFileStorageFactory factory)
{
       this.storage = factory.CreateAwsStorage<Type>();
}

```

# Testing 
To start testing, you need to create a file "appsettings.test.json" in "Cloud Storage".Aws S3.Tests" with the following content:

```
{
  "TestStorage": {
    "Default": {
      "ServiceUrl": "your url",
      "AuthenticationRegion": "your region",
      "AccessKeyId": "your key",
      "SecretAccessKey": "your secret"
    },
    "FakeFile": {
      "BucketName": "your bucket"
    }
}
```
