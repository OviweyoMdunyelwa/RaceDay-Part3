using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.API.Services
{
    // The ONLY class in the whole solution that talks to Azure Blob Storage.
    public class BlobStorageService
    {
        private readonly BlobContainerClient _container;

        public BlobStorageService(IConfiguration configuration)
        {
            string? connectionString = configuration["AzureBlobStorage:ConnectionString"];

            if (string.IsNullOrWhiteSpace(connectionString) ||
                connectionString.StartsWith("YOUR_", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "The Azure Blob Storage connection string is missing.");
            }

            string containerName =
                configuration["AzureBlobStorage:ContainerName"] ?? "raceday-images";

            _container = new BlobContainerClient(connectionString, containerName);
        }

        // Uploads a file and returns its blob URL (this is saved in the database).
        public async Task<string> UploadAsync(IFormFile file, string folder)
        {
            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            string blobName = $"{folder}/{Guid.NewGuid()}{extension}";

            // The container stays PRIVATE (nobody can open it directly from the internet).
            await _container.CreateIfNotExistsAsync();

            BlobClient blob = _container.GetBlobClient(blobName);

            await using Stream stream = file.OpenReadStream();
            await blob.UploadAsync(stream, new BlobHttpHeaders
            {
                ContentType = file.ContentType
            });

            return blob.Uri.ToString();
        }

        // Reads a stored file so the API can hand it to the website.
        public async Task<(Stream Stream, string ContentType)?> DownloadAsync(string blobName)
        {
            BlobClient blob = _container.GetBlobClient(blobName);

            if (!await blob.ExistsAsync())
            {
                return null;
            }

            var download = await blob.DownloadStreamingAsync();
            return (download.Value.Content, download.Value.Details.ContentType);
        }
    }
}
