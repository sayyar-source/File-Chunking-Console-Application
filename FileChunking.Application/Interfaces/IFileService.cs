using FileChunking.Domain.Entities;

namespace FileChunking.Application.Interfaces;
public interface IFileService
{
    Task<Guid> UploadAndDistributeFileAsync(long ChunkSize,string filePath);
    Task<IEnumerable<Guid>> UploadAndDistributeFilesAsync(long ChunkSize, IEnumerable<string> filePaths);
    Task ReassembleFileAsync(Guid fileId, string outputDirectory);
    Task<List<FileMetadata>> ListFilesAsync(); 
}