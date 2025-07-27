using FileChunking.Domain.Entities;

namespace FileChunking.Domain.Interfaces;
public interface IFileRepository
{
    Task SaveFileMetadataAsync(FileMetadata metadata);
    Task<FileMetadata> GetFileMetadataAsync(Guid fileId);
    Task<List<FileMetadata>> ListFilesAsync();
}
