# cloud-storage

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
