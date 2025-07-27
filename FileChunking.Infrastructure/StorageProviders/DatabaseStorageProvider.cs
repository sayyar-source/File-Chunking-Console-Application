using FileChunking.Domain.Entities;
using FileChunking.Domain.Interfaces;
using FileChunking.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace FileChunking.Infrastructure.StorageProviders;
public class DatabaseStorageProvider : IStorageProvider
{
    private readonly FileChunkingDbContext _context;
    private readonly ILogger<DatabaseStorageProvider> _logger;

    public DatabaseStorageProvider(FileChunkingDbContext context, ILogger<DatabaseStorageProvider> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task StoreChunkAsync(Guid chunkId, byte[] data)
    {
        _context.Chunks.Add(new ChunkData { Id = chunkId, Data = data });
        await _context.SaveChangesAsync();
        _logger.LogInformation("Stored chunk {ChunkId} to database", chunkId);
    }

    public async Task<byte[]> RetrieveChunkAsync(Guid chunkId)
    {
        var chunk = await _context.Chunks.FindAsync(chunkId);
        if (chunk == null)
            throw new FileNotFoundException("Chunk not found");
        _logger.LogInformation("Retrieving chunk {ChunkId} from database", chunkId);
        return chunk.Data;
    }
}
