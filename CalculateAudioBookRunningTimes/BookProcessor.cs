using DoenaSoft.MediaInfoHelper.DataObjects.AudioBookMetaXml;
using DoenaSoft.MediaInfoHelper.Readers;
using DoenaSoft.ToolBox.Generics;

namespace DoenaSoft.CalculateAudioBookRunningTimes;

internal sealed class BookProcessor
{
    private static readonly object _lock;

    private readonly bool _reboot;

    private readonly bool _mp4;

    static BookProcessor()
    {
        _lock = new();
    }

    public BookProcessor(bool reboot
        , bool mp4)
    {
        _reboot = reboot;
        _mp4 = mp4;
    }

    internal void Process(DirectoryInfo folder)
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
