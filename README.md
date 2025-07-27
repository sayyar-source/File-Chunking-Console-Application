# **File Chunking Console Application**

A .NET 9 Console Application for splitting **single** or **multiple files** into chunks, storing them across multiple storage providers, and reassembling them with **SHA256 integrity checks**. Built with **Domain-Driven Design (DDD)**, **SOLID principles**, and modern .NET practices, it features **logging** to **Elasticsearch/Kibana** and **auto-migration** for **SQL Server**.

**Repository**: [github.com/your-username/FileChunkingConsoleApp](https://github.com/sayyar-source/File-Chunking-Console-Application)

# **Table of Contents**
- Overview
- Screenshots
- Prerequisites
- Setup Instructions
- Running the Application
- Architectural Choices
- Extra Features
- Contributing
- Reporting Issues
- FAQ
- Troubleshooting

## **Overview**

The **File Chunking Console Application** is a **robust solution** for splitting **single** or **multiple files** into **chunks**, storing them in a **file system** (`C:\chunks`) or **SQL Server database**, and reassembling them with **SHA256 integrity checks**. It features a **console interface**, **auto-migration** for **SQL Server**, and **detailed logging** to **Elasticsearch/Kibana**. Downloaded **files** are saved to `C:\outputs`. Designed for **extensibility**, it supports multiple **storage providers** and **scalable file processing**.

# **Screenshots**

**Console Interface:**

<img width="606" height="371" alt="image" src="https://github.com/user-attachments/assets/f6f38651-e6a2-49a7-a037-6099b4660492" />

**Kibana Logs:**
<img width="1888" height="975" alt="image" src="https://github.com/user-attachments/assets/5d7a60b8-9d46-4ddc-9b41-061c79dedcf8" />

## **Prerequisites**

- **.NET 9 SDK**: Install from [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/9.0).
- **SQL Server**: Local or remote instance (e.g., **SQL Server Express**).
- **Docker**: For running **Elasticsearch** and **Kibana**.
- **Git**: To **clone** the repository.
- **Windows**: For **file system paths** (e.g., `C:\hunks`, `C:\outputs`). For **Linux/macOS**, adjust **paths** (e.g., `/home/chunks`, `/home/outputs`).

## **Setup Instructions**

1. **Clone the Repository**:
   ```bash
   git clone https://github.com/sayyar-source/File-Chunking-Console-Application.git
   cd FileChunkingConsoleApp

2. **Install NuGet Packages**:
   Restore **dependencies** listed in `FileChunkingConsoleApp.csproj`:
    ```bash
     dotnet restore


  Required **packages**:
   - Serilog.AspNetCore (8.0.3)
   - Serilog.Sinks.Console (6.0.0)
   - Serilog.Sinks.File (6.0.0)
   - Serilog.Sinks.Elasticsearch (10.0.0)
   - Microsoft.Extensions.Hosting (9.0.0)
   - Microsoft.EntityFrameworkCore.SqlServer (9.0.0)
   - Microsoft.EntityFrameworkCore.Design (9.0.0)
3. **Configure** appsettings.json: Update appsettings.json with your **SQL Server connection string**:
     ```bash
      {
      "ConnectionStrings": {
      "DefaultConnection": "Server=localhost;Database=FileChunkingDb;Trusted_Connection=True;Encrypt=False;MultipleActiveResultSets=True"
      },
      "Logging": {
      "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
     },
     "FilePath": "logs/log-.txt",
     "ElasticsearchUrl": "http://localhost:9200"
     },
     "Storage": {
     "FileSystemPath": "c:\\chunks",
     "MaxChunkSize": 1048576, // 1 MB
     "OutputDirectory": "c:\\outputs"
     }
     }


  **Note**: Ensure the **SQL Server instance** is accessible. The **ElasticsearchUrl** defaults to `http://localhost:9200` (**Docker default**). **MaxChunkSize** is set to **1 MB** (1048576 bytes).

4. **Set Up Elasticsearch and Kibana**:
   Use the provided `docker-compose.yml` to start **Elasticsearch** and **Kibana**:
   ```bash
   docker-compose up -d
- Verify **Elasticsearch** at `http://localhost:9200`.
- Access **Kibana** at `http://localhost:5601` and create an **index pattern** `filechunking-log-*`.
5. **Generate Database Migrations**:
   Create the initial migration for the `FileChunkingDb` **database**:
   ```bash
 **Note**: The application applies **migrations automatically** at startup.

6. **Ensure Storage Directory Permissions**:
   The `FileSystemStorageProvider` saves **chunks** to `c:\chunks`, and **downloaded files** are saved to `c:\outputs`. Ensure both **directories** are **writable**:
   - **Run** the application as an **administrator**, or
   - Grant **write permissions**:
     ```bash
     icacls "c:\chunks" /grant Users:W
     icacls "c:\outputs" /grant Users:W

# **Running the Application**
1. **Start the Application**:
   ```bash
dotnet run
The application **initializes**, applies **database migrations**, checks **Elasticsearch connectivity**, and displays a **console menu**.

2. **Console Interface**:
      ```bash
    File Chunking Application
    1. Upload single file
    2. Upload multiple files
    3. List files
    4. Download file
    5. Exit
    Select an option:


- **Upload Multiple Files**: Enter comma-separated paths (e.g., c:\file1.txt,c:\file2.pdf) or a directory (e.g., c:\MyFiles).
- **List Files**: View uploaded files with IDs, names, sizes, and chunk counts.
- **Download File**: Enter a file ID and an output path (e.g., c:\outputs\output.txt). A default path is suggested in c:\outputs based on the original file name (e.g., c:\outputs\original.txt).
- **Exit**: Gracefully shuts down (Ctrl+C also works).

3. **View Logs**:
- Logs are written to:
  - Console
  - logs/log-*.txt (daily rolling)
  - asticsearch (filechunking-log-* index)
- In Kibana (http://localhost:5601), view detailed logs (e.g., chunk processing, errors, migrations).

# **Architectural Choices**

The **File Chunking Console Application** follows **modern software design principles** for **maintainability** and **extensibility**:

- **Domain-Driven Design (DDD)**:
  - **Domain entities** (FileMetadata, ChunkMetadata) encapsulate file and chunk metadata.
  - **FileMetadata** stores file details (ID, name, size, checksum, chunks).
  - **ChunkMetadata** tracks chunk details (ID, sequence, size, storage provider).

- **SOLID Principles**:
  - **Single Responsibility**: Each class has a specific role (e.g., FileService for chunking logic, FileRepository for data access).
  - **Open/Closed**: New storage providers can be added without modifying existing code.
  - **Dependency Inversion**: Interfaces (IFileService, IStorageProvider, IFileRepository) decouple components, injected via Microsoft.Extensions.DependencyInjection.

- **Dependency Injection**:
  - Uses Microsoft.Extensions.DependencyInjection for **Inversion of Control (IoC)**.
  - Registers **services** in Program.cs:
    - **Scoped**: IFileService, IFileRepository, ApplicationDbContext.
    - **Singleton**: IStorageProvider implementations.

- **Storage Providers**:
  - Extensible via IStorageProvider **interface**.
  - FileSystemStorageProvider: Saves **chunks** to c:\chunks.
  - DatabaseStorageProvider: Stores **chunks** in **SQL Server**.
  - **Chunks** are distributed across **providers** for **redundancy**.

- **Data Persistence**:
  - **Entity Framework Core** with **SQL Server** (FileChunkingDb).
  - **Auto-migration** at startup ensures the **database schema** is up-to-date.
- **Logging**:
  - **Serilog** with **sinks** for **console**, **file**, and **Elasticsearch**.
  - **Logs** include **migration status**, **file operations**, **chunk details**, and **errors**.
  - Configurable **log levels** via appsettings.json.
- **Integrity Checks**:
  - **SHA256 checksums** computed during **upload** and verified during **download**.
  - **Chunk size validation** prevents **corruption**.
