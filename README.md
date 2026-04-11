# CalculateAudioBookRunningTimes

A command-line utility for scanning audiobook collections and generating detailed metadata XML files with running times, chapter information, and audiobook details.

## Features

- **Automated Metadata Extraction**: Scans MP3 or MP4 audiobook files and extracts detailed metadata
- **Running Time Calculation**: Calculates total running time for audiobooks across multiple files
- **Chapter Detection**: Identifies and catalogs individual chapters/tracks
- **Interactive Metadata Completion**: Prompts for missing author and narrator information
- **Parallel Processing**: Supports batch processing of multiple audiobook folders simultaneously
- **XML Output**: Generates standardized XML metadata files for each audiobook
- **Smart Skip**: Option to skip already processed audiobooks (resume capability)
- **Flexible File Format**: Supports both MP3 (default) and MP4 audiobook formats

## Requirements

- .NET Framework 4.8.1 or higher
- Windows OS (x64)
- [MediaInfo library](https://mediaarea.net/en/MediaInfo) (automatically handled by DoenaSoft.MediaInfoHelper)

## Installation

### From Source

1. Clone the repository:
   ```bash
   git clone https://github.com/DJDoena/CalculateAudioBookRunningTimes.git
   cd CalculateAudioBookRunningTimes
   ```

2. Build the solution:
   ```bash
   dotnet build CalculateAudioBookRunningTimes\CalculateAudioBookRunningTimes.csproj -c Debug
   ```

3. The executable will be in:
   ```
   CalculateAudioBookRunningTimes\bin\x64\Debug\net481\CalculateAudioBookRunningTimes.exe
   ```

### From Release

Download the latest release from the [Releases page](https://github.com/DJDoena/CalculateAudioBookRunningTimes/releases).

## Usage

### Basic Syntax

```bash
CalculateAudioBookRunningTimes.exe <path> [options]
```

### Command-Line Parameters

| Parameter | Description |
|-----------|-------------|
| `<path>` | Path to the audiobook directory (required if `/getpath` not used) |
| `/getpath` | Prompts for path interactively instead of using command-line argument |
| `/r` | **Reboot mode** - Skips folders that already have metadata XML files |
| `/mp4` | Processes MP4 files instead of MP3 files (default: MP3) |
| `/m` | **Multi-folder mode** - Recursively processes multiple audiobook folders with parallel processing |

### Examples

#### Process a Single Audiobook

```bash
CalculateAudioBookRunningTimes.exe "C:\AudioBooks\MyBook"
```

#### Process Multiple Audiobooks in Parallel

```bash
CalculateAudioBookRunningTimes.exe "C:\AudioBooks" /m
```

#### Skip Already Processed Books (Resume)

```bash
CalculateAudioBookRunningTimes.exe "C:\AudioBooks" /m /r
```

#### Process MP4 Audiobooks

```bash
CalculateAudioBookRunningTimes.exe "C:\AudioBooks\MyBook" /mp4
```

#### Interactive Path Entry

```bash
CalculateAudioBookRunningTimes.exe /getpath
```

This will prompt:
```
Enter path:
```

#### Combined Options

```bash
CalculateAudioBookRunningTimes.exe "C:\AudioBooks" /m /r /mp4
```

Process multiple MP4 audiobook folders, skipping those already processed.

## How It Works

1. **Folder Scanning**: The tool scans the specified directory for audio files (MP3 or MP4)

2. **Metadata Extraction**: Uses MediaInfo library to extract:
   - Track duration
   - Audio codec information
   - Bitrate and sample rate
   - Embedded metadata (title, artist, album, etc.)

3. **Interactive Prompts**: If author or narrator information is missing, the tool prompts:
   ```
   Is John Doe (a)uthor, (n)arrator, (b)oth or (s)kip for 'Book Title'?
   ```
   ```
   No author found for 'Book Title'. Please enter author:
   ```

4. **XML Generation**: Creates an XML file named `{FolderName}.xml` containing:
   - Book title
   - Author(s)
   - Narrator(s)
   - Total running time
   - Chapter/track listings with individual durations
   - Audio format details

5. **Parallel Processing**: In multi-folder mode (`/m`), processes up to 4 audiobooks simultaneously

## Output Format

For an audiobook in folder `C:\AudioBooks\GreatBook\`, the tool generates:

```
C:\AudioBooks\GreatBook\GreatBook.xml
```

The XML file follows the AudioBookMeta schema defined by the [DoenaSoft.MediaInfoHelper](https://www.nuget.org/packages/DoenaSoft.MediaInfoHelper/) library.

## Folder Structure Expectations

### Single Book Mode (default)

```
C:\AudioBooks\BookTitle\
├── Chapter01.mp3
├── Chapter02.mp3
├── Chapter03.mp3
└── BookTitle.xml  (generated)
```

### Multi-Book Mode (`/m`)

```
C:\AudioBooks\
├── English\
│   ├── Book1\
│   │   ├── Chapter01.mp3
│   │   └── Book1.xml  (generated)
│   └── Book2\
│       ├── Chapter01.mp3
│       └── Book2.xml  (generated)
├── Deutsch\
│   └── Book3\
│       ├── Chapter01.mp3
│       └── Book3.xml  (generated)
└── Book4\
    ├── Chapter01.mp3
    └── Book4.xml  (generated)
```

Special handling for folders named "English" or "Deutsch" - these are treated as language groupings.

## Additional Tools

### UpdateAudioBookMeta

The solution also includes `UpdateAudioBookMeta`, a utility for updating existing XML metadata files to the current format. This is useful when the XML schema changes or for migrating from older metadata formats.

## Dependencies

- **[DoenaSoft.MediaInfoHelper](https://www.nuget.org/packages/DoenaSoft.MediaInfoHelper/)** (v3.1.12): Provides MediaInfo wrapper functionality for reading audio file metadata
- **.NET Framework 4.8.1**: Required runtime

## Error Handling

The tool includes comprehensive error handling:
- Invalid directory paths are reported
- File processing errors are logged with the book name and error message
- Parallel processing errors are aggregated and reported
- The tool continues processing other books even if one fails

## Performance

- Uses parallel processing with a maximum degree of parallelism of 4
- Thread-safe console output with locking mechanisms
- Efficient metadata caching

## Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.

### Building from Source

```bash
git clone https://github.com/DJDoena/CalculateAudioBookRunningTimes.git
cd CalculateAudioBookRunningTimes
dotnet restore
dotnet build
```

## Version Information

The tool displays its version at startup. Version numbers are automatically generated using the format `yyyy.MM.dd.HHmm` based on build time.

## License

Please check the repository for license information.

## Author

DJDoena (Doena Soft)

## Support

For issues, questions, or suggestions, please use the [GitHub Issues](https://github.com/DJDoena/CalculateAudioBookRunningTimes/issues) page.

## Related Projects

- [DoenaSoft.MediaInfoHelper](https://github.com/DJDoena/MediaInfoHelper) - The underlying library for media file analysis

---

**Note**: This tool is designed for personal audiobook library management and metadata organization. Ensure you have appropriate rights to the audiobook files you process.
