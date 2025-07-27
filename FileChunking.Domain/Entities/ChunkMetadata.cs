namespace FileChunking.Domain.Entities;
public class ChunkMetadata
{
    public Guid Id { get; set; }
    public Guid FileId { get; set; }
    public int SequenceNumber { get; set; }
    public long Size { get; set; }
    public string StorageProviderId { get; set; }
}
