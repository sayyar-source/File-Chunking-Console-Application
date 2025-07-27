namespace FileChunking.Domain.Entities;
public class ChunkData
{
    public Guid Id { get; set; }
    public byte[] Data { get; set; }
}
