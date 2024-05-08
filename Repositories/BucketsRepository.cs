using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;

namespace ZachPerini_6._3A_HA.Repositories
{
    public class BucketsRepository
    {
        private string _projectId;
        private string _bucketName;
        private readonly StorageClient _storageClient;

        public BucketsRepository(string projectId, string bucketName)
        {
            _storageClient = StorageClient.Create();
            _projectId = projectId;
            _bucketName = bucketName;
        }

        public async Task<Google.Apis.Storage.v1.Data.Object> UploadFile(string filename, MemoryStream ms)
        {
            var storage = StorageClient.Create();
            return await storage.UploadObjectAsync(_bucketName, filename, "application/octet-stream", ms);
        }

        public async Task<Google.Apis.Storage.v1.Data.Object> GrantAccess(string filename, string recipient)
        {
            var storage = StorageClient.Create();
            var storageObject = storage.GetObject(_bucketName, filename, new GetObjectOptions
            {
                Projection = Projection.Full
            });

            storageObject.Acl.Add(new ObjectAccessControl
            {
                Bucket = _bucketName,
                Entity = $"user-{recipient}",
                Role = "READER",
            });
            return await storage.UpdateObjectAsync(storageObject);
        }

        public async Task<byte[]> DownloadFile(string filename)
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    await _storageClient.DownloadObjectAsync(_bucketName, filename, memoryStream);
                    return memoryStream.ToArray();
                }
            }
            catch (Google.GoogleApiException ex)
            {
                // Handle the exception (e.g., log, return null, etc.)
                Console.WriteLine($"Error downloading file {filename}: {ex.Message}");
                return null;
            }
        }
    }
}
