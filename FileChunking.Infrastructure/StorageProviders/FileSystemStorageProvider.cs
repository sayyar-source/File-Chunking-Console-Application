namespace FileChunking.Infrastructure.StorageProviders;

using FileChunking.Domain.Interfaces;
using Microsoft.Extensions.Logging;
public class FileSystemStorageProvider : IStorageProvider
{
    private readonly string _storagePath; 
    private readonly ILogger _logger;
    public FileSystemStorageProvider(string storagePath, ILogger<FileSystemStorageProvider> logger)
    {
        _storagePath = storagePath;
        _logger = logger;
        Directory.CreateDirectory(storagePath);
    }
   public async Task StoreChunkAsync(Guid chunkId, byte[] data)
    {
        var path = Path.Combine(_storagePath, chunkId.ToString());
        await File.WriteAllBytesAsync(path, data);
        _logger.LogInformation("Stored chunk {ChunkId} to filesystem", chunkId);
    } 

    public async Task<byte[]> RetrieveChunkAsync(Guid chunkId)
    {
        var path = Path.Combine(_storagePath, chunkId.ToString());
        if (!File.Exists(path))
            throw new FileNotFoundException("Chunk not found", path);
        _logger.LogInformation("Retrieving chunk {ChunkId} from filesystem", chunkId);
        return await File.ReadAllBytesAsync(path);
    }
}
