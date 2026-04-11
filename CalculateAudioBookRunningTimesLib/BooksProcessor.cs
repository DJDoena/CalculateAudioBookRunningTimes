using DoenaSoft.AbstractionLayer.IOServices;

namespace DoenaSoft.CalculateAudioBookRunningTimes;

public sealed class BooksProcessor
{
    public BookProcessor _bookProcessor;

    public BooksProcessor(BookProcessor bookProcessor)
    {
        _bookProcessor = bookProcessor;
    }

    public void Process(IFolderInfo rootFolder)
    {
        var folders = rootFolder.GetFolders("*.*", SearchOption.TopDirectoryOnly);

        Parallel.ForEach(folders, new ParallelOptions() { MaxDegreeOfParallelism = 4 }, folder =>
        {
            if (rootFolder.Name is "English" or "Deutsch")
            {
                this.Process(folder);
            }
            else if (folder.GetFolders("*.*", SearchOption.TopDirectoryOnly).Any())
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
