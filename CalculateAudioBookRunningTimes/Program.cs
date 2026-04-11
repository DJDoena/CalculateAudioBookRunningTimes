namespace DoenaSoft.CalculateAudioBookRunningTimes;

public static class Program
{
    private static BookProcessor _processor;

    public static void Main(string[] args)
    {
        Console.WriteLine($"v{typeof(Program).Assembly.GetName().Version}");

        if (args.Length == 0)
        {
            Console.WriteLine("Invalid parameter count.");

            return;
        }

        string path;
        if (args.Any(a => a == "/getpath"))
        {
            do
            {
                Console.WriteLine("Enter path:");

                path = Console.ReadLine().Trim().Trim('"');

            } while (!Directory.Exists(path));
        }
        else
        {
            path = args[0];
        }

        var reboot = args.Any(a => a == "/r");

        var mp4 = args.Any(a => a == "/mp4");

        var folder = new DirectoryInfo(path);

        if (!folder.Exists)
        {
            Console.WriteLine($"'{folder.FullName}' is not a valid directory.");

            return;
        }

        _processor = new BookProcessor(reboot, mp4);

        if (args.Any(a => a == "/m"))
        {
            ProcessBooks(folder);
        }
        else
        {
            _processor.Process(folder);
        }
    }

    private static void ProcessBooks(DirectoryInfo rootFolder)
    {
        var folders = rootFolder.GetDirectories("*.*", SearchOption.TopDirectoryOnly);

        Parallel.ForEach(folders, new ParallelOptions() { MaxDegreeOfParallelism = 4 }, folder =>
        {
            if (rootFolder.Name is "English" or "Deutsch")
            {
                ProcessBooks(folder);
            }
            else if (folder.GetDirectories("*.*", SearchOption.TopDirectoryOnly).Any())
            {
                ProcessBooks(folder);
            }
            else
            {
                _processor.Process(folder);
            }
        });
    }
}