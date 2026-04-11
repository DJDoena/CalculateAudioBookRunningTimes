# CalculateAudioBookRunningTimes Library

A .NET library for extracting and calculating audiobook metadata including running times, chapters, authors, and narrators from MP3 and MP4 audio files.

## Overview

This library provides a programmatic interface for processing audiobook collections and generating structured metadata. It abstracts the core functionality of the CalculateAudioBookRunningTimes command-line tool into a reusable library that can be integrated into other applications.

## Features

- **Audiobook Metadata Extraction**: Automatically extracts metadata from MP3 and MP4 audio files
- **Running Time Calculation**: Computes total running time across multiple audio files
- **Chapter Detection**: Identifies and catalogs individual chapters or tracks
- **Customizable Interaction**: Pluggable interface for handling user prompts and logging
- **Batch Processing**: Process multiple audiobook folders in parallel
- **XML Serialization**: Generates standardized AudioBookMeta XML files
- **Abstraction Layer Support**: Uses DoenaSoft.AbstractionLayer.IO for file system operations
- **Thread-Safe**: Designed for concurrent processing scenarios

## Installation

### Via NuGet Package Manager

```powershell
Install-Package DoenaSoft.CalculateAudioBookRunningTimes
```

### Via .NET CLI

```bash
dotnet add package DoenaSoft.CalculateAudioBookRunningTimes
```

### Via PackageReference

```xml
<PackageReference Include="DoenaSoft.CalculateAudioBookRunningTimes" Version="*" />
```

## Target Framework

- .NET Framework 4.8.1

## Dependencies

- **DoenaSoft.AbstractionLayer.IO** (v5.0.1): Provides file system abstraction
- **DoenaSoft.MediaInfoHelper** (v3.1.12): MediaInfo wrapper for reading audio metadata

## Quick Start

### Basic Usage

```csharp
using DoenaSoft.CalculateAudioBookRunningTimes;
using DoenaSoft.AbstractionLayer.IOServices.DefaultImplementations;

// Create an interaction handler
IInteraction interaction = new ConsoleInteraction();

// Create a book processor
var processor = new BookProcessor(
    reboot: false,    // Set to true to skip already processed books
    mp4: false,       // Set to true for MP4 files instead of MP3
    interaction: interaction
);

// Get folder info
var ioServices = new IOServices();
var folder = ioServices.GetFolderInfo(@"C:\AudioBooks\MyBook");

// Process the audiobook
processor.Process(folder);
```

### Batch Processing Multiple Audiobooks

```csharp
using DoenaSoft.CalculateAudioBookRunningTimes;
using DoenaSoft.AbstractionLayer.IOServices.DefaultImplementations;

// Create interaction and book processor
IInteraction interaction = new ConsoleInteraction();
var bookProcessor = new BookProcessor(false, false, interaction);

// Create books processor for batch operations
var booksProcessor = new BooksProcessor(bookProcessor);

// Get root folder
var ioServices = new IOServices();
var rootFolder = ioServices.GetFolderInfo(@"C:\AudioBooks");

// Process all audiobooks in parallel
booksProcessor.Process(rootFolder);
```

## Core Components

### IInteraction Interface

Defines the contract for user interaction and logging:

```csharp
public interface IInteraction
{
    void WriteLine(string message = null);
    void Write(string message);
    string ReadLine();
}
```

**Purpose**: Allows consumers to customize how the library handles:
- User prompts for missing metadata (author, narrator)
- Progress logging
- Error reporting

**Example Implementation**:

```csharp
public class ConsoleInteraction : IInteraction
{
    public void WriteLine(string message = null)
        => Console.WriteLine(message);

    public void Write(string message)
        => Console.Write(message);

    public string ReadLine()
        => Console.ReadLine();
}
```

### BookProcessor Class

Processes a single audiobook folder:

**Constructor Parameters**:
- `reboot` (bool): Skip folders that already have metadata XML files
- `mp4` (bool): Process MP4 files instead of MP3 files
- `interaction` (IInteraction): Handler for user interaction and logging

**Methods**:
- `Process(IFolderInfo folder)`: Scans the folder, extracts metadata, and generates XML

**Behavior**:
1. Checks for existing metadata file (skips if `reboot` is true)
2. Scans for audio files (*.mp3 or *.mp4)
3. Extracts metadata using MediaInfo
4. Prompts for missing author/narrator information via IInteraction
5. Generates `{FolderName}.xml` in the audiobook folder

### BooksProcessor Class

Processes multiple audiobook folders in parallel:

**Constructor Parameters**:
- `bookProcessor` (BookProcessor): The processor to use for individual books

**Methods**:
- `Process(IFolderInfo rootFolder)`: Recursively processes audiobook collections

**Behavior**:
- Parallelizes processing (max 4 concurrent operations)
- Handles special folder names ("English", "Deutsch") as language groupings
- Recursively processes nested folder structures

### RootItemXsltSerializerDataProvider Class

Provides XSLT stylesheet integration for XML output:

**Purpose**: Embeds an XSLT stylesheet in the generated XML files for browser-based rendering

**Methods**:
- `GetPrefix()`: Returns XML header with stylesheet declaration
- `GetSuffix()`: Returns embedded XSLT stylesheet

## Advanced Scenarios

### Custom Interaction Handler

Create a custom interaction handler for GUI applications:

```csharp
public class GuiInteraction : IInteraction
{
    private readonly ILogger _logger;
    private readonly IDialogService _dialogService;

    public GuiInteraction(ILogger logger, IDialogService dialogService)
    {
        _logger = logger;
        _dialogService = dialogService;
    }

    public void WriteLine(string message = null)
    {
        _logger.LogInformation(message);
    }

    public void Write(string message)
    {
        _logger.LogInformation(message);
    }

    public string ReadLine()
    {
        return _dialogService.ShowInputDialog("Input Required", "");
    }
}
```

### Silent Processing with Pre-configured Metadata

```csharp
public class SilentInteraction : IInteraction
{
    private readonly IDictionary<string, string> _predefinedAuthors;
    private readonly IDictionary<string, string> _predefinedNarrators;

    public void WriteLine(string message = null)
    {
        // Log to file or ignore
    }

    public void Write(string message)
    {
        // Log to file or ignore
    }

    public string ReadLine()
    {
        // Return pre-configured values or defaults
        return string.Empty;
    }
}
```

### Error Handling

The library handles errors gracefully:

```csharp
try
{
    processor.Process(folder);
}
catch (Exception ex)
{
    // Errors are logged via IInteraction.WriteLine
    // Format: "Error processing '{folder}' {errorMessage}"
}
```

All exceptions during processing are caught and logged, allowing batch operations to continue.

## Output Format

Generated XML follows the AudioBookMeta schema from DoenaSoft.MediaInfoHelper:

**Example Output** (`BookTitle.xml`):

```xml
<?xml version="1.0" encoding="utf-8"?>
<?xml-stylesheet type="text/xml" href="#stylesheet"?>
<!DOCTYPE doc [
<!ATTLIST xsl:stylesheet
    id    ID    #REQUIRED>
]>
<doc>
    <Mp3Meta>
        <Title>Book Title</Title>
        <Author>John Doe</Author>
        <Narrator>Jane Smith</Narrator>
        <RunningTime>08:45:32</RunningTime>
        <!-- Additional metadata -->
    </Mp3Meta>
    <!-- Embedded XSLT stylesheet for browser rendering -->
</doc>
```

## Folder Structure Expectations

### Single Audiobook

```
C:\AudioBooks\BookTitle\
|-- Chapter01.mp3
|-- Chapter02.mp3
|-- Chapter03.mp3
+-- BookTitle.xml  (generated)
```

### Multiple Audiobooks

```
C:\AudioBooks\
|-- Book1\
|   |-- Chapter01.mp3
|   +-- Book1.xml  (generated)
|-- Book2\
|   |-- Chapter01.mp3
|   +-- Book2.xml  (generated)
+-- English\
    +-- Book3\
        |-- Chapter01.mp3
        +-- Book3.xml  (generated)
```

**Special Handling**:
- Folders named "English" or "Deutsch" are treated as language groupings
- Nested folder structures are processed recursively

## Thread Safety

All logging and user interaction operations are protected with locks to ensure thread safety during parallel processing.

## API Documentation

Full API documentation is generated via XML documentation comments. Enable XML documentation in your IDE for IntelliSense support.

## Building from Source

```bash
git clone https://github.com/DJDoena/CalculateAudioBookRunningTimes.git
cd CalculateAudioBookRunningTimes
dotnet build CalculateAudioBookRunningTimesLib/CalculateAudioBookRunningTimesLib.csproj
```

### Creating a NuGet Package

```bash
dotnet pack CalculateAudioBookRunningTimesLib/CalculateAudioBookRunningTimesLib.csproj -c Release
```

The package will be created in:
```
CalculateAudioBookRunningTimesLib\bin\Release\DoenaSoft.CalculateAudioBookRunningTimes.{version}.nupkg
```

## Versioning

The library uses automatic versioning based on build time: `yyyy.MM.dd.HHmm`

## Contributing

Contributions are welcome! Please:
1. Fork the repository
2. Create a feature branch
3. Submit a pull request

## Support

- **GitHub Issues**: https://github.com/DJDoena/CalculateAudioBookRunningTimes/issues
- **Repository**: https://github.com/DJDoena/CalculateAudioBookRunningTimes

## License

MIT License - See repository for details

## Related Projects

- **CalculateAudioBookRunningTimes CLI**: Command-line tool using this library
- **DoenaSoft.MediaInfoHelper**: Underlying media analysis library
- **DoenaSoft.AbstractionLayer.IO**: File system abstraction layer

## Author

DJDoena (Doena Soft)

---

**Note**: This library is designed for personal and professional audiobook library management. Ensure you have appropriate rights to process audiobook files.