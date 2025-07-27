using FileChunking.Application.Interfaces;
using FileChunking.Domain.Entities;
using FileChunking.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FileChunking.Application.Services;
public class FileService : IFileService
{
    private readonly IFileRepository _fileRepository;
    private readonly IEnumerable<IStorageProvider> _storageProviders;
    private readonly ILogger<FileService> _logger;
    //private const int CHUNK_SIZE = 1024 * 1024; // 1MB

    public FileService(IFileRepository fileRepository, IEnumerable<IStorageProvider> storageProviders, ILogger<FileService> logger)
    {
        _fileRepository = fileRepository;
        _storageProviders = storageProviders;
        _logger = logger;
    }

    public async Task<Guid> UploadAndDistributeFileAsync(long ChunkSize,string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found", filePath);

        _logger.LogInformation("Starting file upload: {FilePath}", filePath);
        var fileId = Guid.NewGuid();
        var fileName = Path.GetFileName(filePath);
        var metadata = new FileMetadata { Id = fileId, FileName = fileName };
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var chunks = new List<ChunkMetadata>();
        int sequence = 0;
        byte[] buffer = new byte[ChunkSize];
        int bytesRead;

        while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            var chunkData = bytesRead == buffer.Length ? buffer : buffer.Take(bytesRead).ToArray();
            var chunkId = Guid.NewGuid();
            var provider = _storageProviders.ElementAt(sequence % _storageProviders.Count());
            await provider.StoreChunkAsync(chunkId, chunkData);
            chunks.Add(new ChunkMetadata { Id = chunkId, FileId = fileId, SequenceNumber = sequence++, Size = bytesRead, StorageProviderId = provider.GetType().Name });
            _logger.LogInformation("Stored chunk {SequenceNumber} for file {FileName}", sequence, fileName);
        }

        fileStream.Position = 0;
        metadata.FileSize = fileStream.Length;
        metadata.Checksum = Convert.ToBase64String(sha256.ComputeHash(fileStream));
        metadata.Chunks = chunks;
        await _fileRepository.SaveFileMetadataAsync(metadata);
        _logger.LogInformation("File upload completed: {FileName}, ID: {FileId}", fileName, fileId);
        return fileId;
    }

    public async Task<IEnumerable<Guid>> UploadAndDistributeFilesAsync(long ChunkSize, IEnumerable<string> filePaths)
    {
        var fileIds = new List<Guid>();
        foreach (var filePath in filePaths)
        {
            try
            {
                var fileId = await UploadAndDistributeFileAsync(ChunkSize,filePath);
                fileIds.Add(fileId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload file {FilePath}: {Message}", filePath, ex.Message);
                Console.WriteLine($"Error uploading {filePath}: {ex.Message}");
            }
        }
        _logger.LogInformation("Completed uploading {FileCount} files with IDs: {FileIds}", fileIds.Count, string.Join(", ", fileIds));
        return fileIds;
    }
    public async Task ReassembleFileAsync(Guid fileId, string outputDirectory)
    
    {
        _logger.LogInformation("Reassembling file: {FileId}", fileId);
        var metadata = await _fileRepository.GetFileMetadataAsync(fileId);
        string outputPath = Path.Combine(outputDirectory, metadata.FileName);
        // Ensure the directory exists
        Directory.CreateDirectory(outputDirectory);
        using var outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        using var memoryStream = new MemoryStream();

        foreach (var chunk in metadata.Chunks.OrderBy(c => c.SequenceNumber))
        {
            var provider = _storageProviders.First(p => p.GetType().Name == chunk.StorageProviderId);
            var chunkData = await provider.RetrieveChunkAsync(chunk.Id);
            await outputStream.WriteAsync(chunkData, 0, chunkData.Length);
            await memoryStream.WriteAsync(chunkData, 0, chunkData.Length);
            _logger.LogInformation("Retrieved chunk {SequenceNumber} for file {FileId}", chunk.SequenceNumber, fileId);
        }

        memoryStream.Position = 0;
        var computedChecksum = Convert.ToBase64String(sha256.ComputeHash(memoryStream));
        if (computedChecksum != metadata.Checksum)
            throw new InvalidOperationException("File integrity check failed");

        _logger.LogInformation("File reassembled successfully: {FileId} to {OutputPath}", fileId, outputPath);
    }

    public async Task<List<FileMetadata>> ListFilesAsync()
    {
        return await _fileRepository.ListFilesAsync();
    }
}
