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
