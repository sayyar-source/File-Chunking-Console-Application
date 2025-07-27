using FileChunking.Domain.Entities;
using FileChunking.Domain.Interfaces;
using FileChunking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FileChunking.Infrastructure.Repositories;
public class FileRepository : IFileRepository
{
    private readonly FileChunkingDbContext _context;
    private readonly ILogger<FileRepository> _logger;

    public FileRepository(FileChunkingDbContext context, ILogger<FileRepository> logger)
    {
        _context = context;
        _logger = logger;
    }
    public async Task SaveFileMetadataAsync(FileMetadata metadata)
    {
        _context.FileMetadata.Add(metadata);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Saved metadata for file {FileId}", metadata.Id);
    }

    public async Task<FileMetadata> GetFileMetadataAsync(Guid fileId)
    {
        var metadata = await _context.FileMetadata
            .Include(f => f.Chunks)
            .FirstOrDefaultAsync(f => f.Id == fileId);
        if (metadata == null)
            throw new FileNotFoundException("File metadata not found");
        _logger.LogInformation("Retrieved metadata for file {FileId}", fileId);
        return metadata;
    }

    public async Task<List<FileMetadata>> ListFilesAsync()
    {
        return await _context.FileMetadata
            .Include(f => f.Chunks)
            .ToListAsync();
    }
}
