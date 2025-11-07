using DoenaSoft.MediaInfoHelper.DataObjects.AudioBookMetaXml;
using DoenaSoft.MediaInfoHelper.Readers;
using DoenaSoft.ToolBox.Generics;

namespace DoenaSoft.CalculateAudioBookRunningTimes;

public static class Program
{
    private static readonly object _lock;

    private static bool _reboot;

    private static bool _mp4;

    static Program()
    {
        _lock = new object();
    }

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

        _reboot = args.Any(a => a == "/r");

        _mp4 = args.Any(a => a == "/mp4");

        var folder = new DirectoryInfo(path);

        if (!folder.Exists)
        {
            Console.WriteLine($"'{folder.FullName}' is not a valid directory.");

            return;
        }

        if (args.Any(a => a == "/m"))
        {
            ProcessBooks(folder);
        }
        else
        {
            ProcessBook(folder);
        }
    }

    private static void ProcessBooks(DirectoryInfo rootFolder)
    {
        var folders = rootFolder.GetDirectories("*.*", SearchOption.TopDirectoryOnly);

        Parallel.ForEach(folders, new ParallelOptions() { MaxDegreeOfParallelism = 4 }, folder =>
        {
            if (rootFolder.Name == "English" || rootFolder.Name == "Deutsch")
            {
                ProcessBooks(folder);
            }
            else if (folder.GetDirectories("*.*", SearchOption.TopDirectoryOnly).Any())
            {
                ProcessBooks(folder);
            }
            else
            {
                ProcessBook(folder);
            }
        });
    }

    private static void ProcessBook(DirectoryInfo folder)
    {
        var metaFileName = Path.Combine(folder.FullName, $"{folder.Name}.xml");

        if (_reboot && File.Exists(metaFileName))
        {
            return;
        }

        try
        {
            lock (_lock)
            {
                Console.WriteLine($"Processing '{folder.Name}'.");
            }

            var filePattern = _mp4
                ? "*.mp4"
                : "*.mp3";

            var mp3Meta = (new AudioBookReader(GetRole
                , (bookTitle) => GetName(bookTitle, "author")
                , (bookTitle) => GetName(bookTitle, "narrator")
                , Log))
                .GetMeta(folder, filePattern);

            (new XsltSerializer<AudioBookMeta>(new RootItemXsltSerializerDataProvider())).Serialize(metaFileName, mp3Meta);
        }
        catch (AggregateException aggrEx)
        {
            lock (_lock)
            {
                Console.WriteLine($"Error processing '{folder.Name}' {aggrEx.InnerException?.Message ?? aggrEx.Message}");
            }
        }
        catch (Exception ex)
        {
            lock (_lock)
            {
                Console.WriteLine($"Error processing '{folder.Name}' {ex.Message}");
            }
        }
    }

    private static BookRole GetRole(string bookTitle, string person)
    {
        var answer = string.Empty;

        lock (_lock)
        {
            while (answer != "a" && answer != "n" && answer != "b" && answer != "s")
            {
                Console.Write($"Is {person} (a)uthor, (n)arrator, (b)oth or (s)kip for '{bookTitle}'? ");

                answer = Console.ReadLine();
            }
        }

        switch (answer)
        {
            case "a":
                {
                    return BookRole.Author;
                }
            case "n":
                {
                    return BookRole.Narrator;
                }
            case "b":
                {
                    return BookRole.Author | BookRole.Narrator;
                }
            default:
                {
                    return BookRole.Undefined;
                }
        }
    }

    private static string GetName(string bookTitle, string role)
    {
        var answer = string.Empty;

        lock (_lock)
        {
            Console.Write($"No {role} found for '{bookTitle}'. Please enter {role}: ");

            answer = Console.ReadLine();
        }

        return answer;
    }

    private static void Log(string message)
    {
        lock (_lock)
        {
            Console.WriteLine(message);
        }
    }
}