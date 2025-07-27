using FileChunking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FileChunking.Infrastructure.Data;
public class FileChunkingDbContext : DbContext
{
    public DbSet<FileMetadata> FileMetadata { get; set; }
    public DbSet<ChunkMetadata> ChunkMetadata { get; set; }
    public DbSet<ChunkData> Chunks { get; set; }
    public FileChunkingDbContext(DbContextOptions<FileChunkingDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FileMetadata>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired();
            entity.Property(e => e.Checksum).IsRequired();
            entity.HasMany(e => e.Chunks).WithOne().HasForeignKey("FileId");
        });

        modelBuilder.Entity<ChunkMetadata>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileId).IsRequired();
            entity.Property(e => e.SequenceNumber).IsRequired();
            entity.Property(e => e.StorageProviderId).IsRequired();
            entity.Property(e => e.Size).IsRequired();
        });

        modelBuilder.Entity<ChunkData>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Data).IsRequired();
        });
    }
}
