namespace FileChunking.Domain.Interfaces;
public interface IStorageProvider
{
    Task StoreChunkAsync(Guid chunkId, byte[] data);
    Task<byte[]> RetrieveChunkAsync(Guid chunkId);
}
