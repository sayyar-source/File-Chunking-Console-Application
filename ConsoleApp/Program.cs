using FileChunking.Application.Interfaces;
using FileChunking.Application.Services;
using FileChunking.Domain.Interfaces;
using FileChunking.Infrastructure.Data;
using FileChunking.Infrastructure.Repositories;
using FileChunking.Infrastructure.StorageProviders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace FileChunkingConsoleApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Setup DI and logging
            var builder = Host.CreateDefaultBuilder(args);

            // Build configuration
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) 
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            string connectionString = config.GetConnectionString("DefaultConnection");
            string fileSystemPath = config.GetValue<string>("Storage:FileSystemPath");
            string outputDirectory = config.GetValue<string>("Storage:OutputDirectory");
            long maxChunkSize = config.GetValue<long>("Storage:MaxChunkSize");

            // Enable Serilog with Elasticsearch
            builder.UseSerilog((context, configuration) =>
            {
                configuration
                    .MinimumLevel.Information()
                    .WriteTo.Console()
                    .WriteTo.File(
                        path: config.GetValue<string>("Logging:FilePath", "logs/log-.txt"),
                        rollingInterval: RollingInterval.Day)
                    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(
                        new Uri(config.GetValue<string>("Logging:ElasticsearchUrl", "http://localhost:9200")))
                    {
                        AutoRegisterTemplate = true,
                        IndexFormat = "filechunking-log-{0:yyyy.MM.dd}",
                        MinimumLogEventLevel = Serilog.Events.LogEventLevel.Information,
                        FailureCallback = (logEvent, ex) =>
                        {
                            Console.WriteLine($"Elasticsearch sink failed: {ex.Message}");
                        }
                    });
            });

            builder.ConfigureServices((context, services) =>
            {
                if (string.IsNullOrEmpty(connectionString))
                    throw new InvalidOperationException("Database connection string is missing");

                services.AddDbContext<FileChunkingDbContext>(options =>
                    options.UseSqlServer(connectionString));
                services.AddSingleton<IStorageProvider>(sp =>
                    new FileSystemStorageProvider(
                        fileSystemPath,
                        sp.GetRequiredService<ILogger<FileSystemStorageProvider>>()));
                services.AddSingleton<IStorageProvider>(sp =>
                    new DatabaseStorageProvider(
                        sp.GetRequiredService<FileChunkingDbContext>(),
                        sp.GetRequiredService<ILogger<DatabaseStorageProvider>>()));
                services.AddScoped<IFileRepository, FileRepository>();
                services.AddScoped<IFileService, FileService>();
            });

            var host = builder.Build();

            // Apply database migrations
            using (var scope = host.Services.CreateScope())
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                try
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<FileChunkingDbContext>();
                    logger.LogInformation("Applying database migrations...");
                    await dbContext.Database.MigrateAsync();
                    logger.LogInformation("Database migrations applied successfully.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to apply database migrations: {Message}", ex.Message);
                    Console.WriteLine($"Migration failed: {ex.Message}");
                    return; // Exit if migrations fail
                }
            }

            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) =>
            {
                Console.WriteLine("Shutting down...");
                cts.Cancel();
                e.Cancel = true;
            };

            // Console interface
            while (!cts.Token.IsCancellationRequested)
            {
                Console.Clear();
                Console.WriteLine("\nFile Chunking Application");
                Console.WriteLine("1. Upload single file");
                Console.WriteLine("2. Upload multiple files");
                Console.WriteLine("3. List files");
                Console.WriteLine("4. Download file");
                Console.WriteLine("5. Exit");
                Console.Write("Select an option: ");
                var option = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(option))
                {
                    Console.WriteLine("Please select a valid option.");
                    Console.ReadKey();
                    continue;
                }

                using var scope = host.Services.CreateScope();
                var fileService = scope.ServiceProvider.GetRequiredService<IFileService>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

                try
                {
                    switch (option)
                    {
                        case "1":
                            Console.Write("Enter file path: ");
                            var singleFilePath = Console.ReadLine()?.Trim();
                            if (string.IsNullOrEmpty(singleFilePath) || !File.Exists(singleFilePath))
                            {
                                Console.WriteLine("Invalid or non-existent file path.");
                                break;
                            }
                           
                            var singleFileId = await fileService.UploadAndDistributeFileAsync(maxChunkSize, singleFilePath);
                            Console.WriteLine($"File uploaded successfully. ID: {singleFileId}");
                            break;

                        case "2":
                            Console.Write("Enter file paths (comma-separated) or directory path: ");
                            var input = Console.ReadLine()?.Trim();
                            if (string.IsNullOrEmpty(input))
                            {
                                Console.WriteLine("No input provided.");
                                break;
                            }
                            IEnumerable<string> filePaths;
                            if (Directory.Exists(input))
                            {
                                filePaths = Directory.GetFiles(input);
                                if (!filePaths.Any())
                                {
                                    Console.WriteLine("No files found in directory.");
                                    break;
                                }
                            }
                            else
                            {
                                filePaths = input.Split(',', StringSplitOptions.TrimEntries)
                                    .Where(path => !string.IsNullOrEmpty(path) && File.Exists(path));
                                if (!filePaths.Any())
                                {
                                    Console.WriteLine("No valid file paths provided.");
                                    break;
                                }
                            }
                            var fileIds = await fileService.UploadAndDistributeFilesAsync(maxChunkSize, filePaths);
                            Console.WriteLine($"Uploaded {fileIds.Count()} files with IDs: {string.Join(", ", fileIds)}");
                            break;

                        case "3":
                            var files = await fileService.ListFilesAsync();
                            Console.WriteLine("Uploaded files:");
                            if (!files.Any())
                                Console.WriteLine("No files found.");
                            foreach (var file in files)
                                Console.WriteLine($"ID: {file.Id}, Name: {file.FileName}, Size: {file.FileSize} bytes, Chunks: {file.Chunks.Count}");
                            break;

                        case "4":
                            Console.Write("Enter file ID: ");
                            var idInput = Console.ReadLine()?.Trim();
                            if (!Guid.TryParse(idInput, out var downloadFileId))
                            {
                                Console.WriteLine("Invalid file ID.");
                                break;
                            }
         
                            await fileService.ReassembleFileAsync(downloadFileId, outputDirectory);
                            Console.WriteLine("File downloaded successfully.");
                            break;

                        case "5":
                            cts.Cancel();
                            break;

                        default:
                            Console.WriteLine("Invalid option.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error occurred during operation: {Message}", ex.Message);
                    Console.WriteLine($"Error: {ex.Message}");
                }

                if (!cts.Token.IsCancellationRequested)
                {
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                }
            }

            await host.StopAsync();
        }
    }
}