using System.Collections.Generic;

namespace FileChunking.Domain.Entities;
public class FileMetadata
{ 
    public Guid Id { get; set; }
    public string FileName { get; set; } 
    public long FileSize { get; set; } 
    public string Checksum { get; set; } 
    public List<ChunkMetadata> Chunks { get; set; }
}
