namespace DoenaSoft.CalculateAudioBookRunningTimes;

/// <summary>
/// Processes multiple audiobook folders in parallel.
/// </summary>
public sealed class BooksProcessor
{
    /// <summary>
    /// The processor used for handling individual audiobook folders.
    /// </summary>
    public BookProcessor _bookProcessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="BooksProcessor"/> class.
    /// </summary>
    /// <param name="bookProcessor">The processor to use for individual audiobook folders.</param>
    public BooksProcessor(BookProcessor bookProcessor)
    {
        _bookProcessor = bookProcessor;
    }

    /// <summary>
    /// Processes all subfolders in the specified root folder recursively.
    /// </summary>
    /// <param name="rootFolder">The root folder to process.</param>
    public void Process(DirectoryInfo rootFolder)
    {
        var folders = rootFolder.GetDirectories("*.*", SearchOption.TopDirectoryOnly);

        Parallel.ForEach(folders, new ParallelOptions() { MaxDegreeOfParallelism = 4 }, folder =>
        {
            if (rootFolder.Name is "English" or "Deutsch")
            {
                this.Process(folder);
            }
            else if (folder.GetDirectories("*.*", SearchOption.TopDirectoryOnly).Any())
            {
                this.Process(folder);
            }
            else
            {
                _bookProcessor.Process(folder);
            }
        });
    }
}