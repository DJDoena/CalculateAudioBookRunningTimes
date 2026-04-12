using DoenaSoft.AbstractionLayer.UI.Contracts;
using DoenaSoft.MediaInfoHelper.DataObjects.AudioBookMetaXml;
using DoenaSoft.MediaInfoHelper.Readers;
using DoenaSoft.ToolBox.Generics;

namespace DoenaSoft.CalculateAudioBookRunningTimes;

/// <summary>
/// Processes individual audiobook folders to extract and save metadata.
/// </summary>
public sealed class BookProcessor
{
    private static readonly object _lock;

    private readonly bool _reboot;

    private readonly bool _mp4;

    private readonly IInteraction _interaction;

    static BookProcessor()
    {
        _lock = new();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BookProcessor"/> class.
    /// </summary>
    /// <param name="reboot">If true, skips processing folders that already have metadata files.</param>
    /// <param name="mp4">If true, processes MP4 files; otherwise processes MP3 files.</param>
    /// <param name="interaction">The interaction interface for user communication.</param>
    public BookProcessor(bool reboot
        , bool mp4
        , IInteraction interaction)
    {
        _reboot = reboot;
        _mp4 = mp4;
        _interaction = interaction;
    }

    /// <summary>
    /// Processes a folder to extract audiobook metadata and save it to an XML file.
    /// </summary>
    /// <param name="folder">The folder containing audiobook files.</param>
    public void Process(DirectoryInfo folder)
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
                _interaction.WriteLine($"Processing '{folder.Name}'.");
            }

            var filePattern = _mp4
                ? "*.mp4"
                : "*.mp3";

            var mp3Meta = (new AudioBookReader(this.GetRole
                , (bookTitle) => this.GetName(bookTitle, "author")
                , (bookTitle) => this.GetName(bookTitle, "narrator")
                , this.Log))
                .GetMeta(folder.FullName, filePattern);

            (new XsltSerializer<AudioBookMeta>(new RootItemXsltSerializerDataProvider())).Serialize(metaFileName, mp3Meta);
        }
        catch (AggregateException aggrEx)
        {
            lock (_lock)
            {
                _interaction.WriteLine($"Error processing '{folder.Name}' {aggrEx.InnerException?.Message ?? aggrEx.Message}");
            }
        }
        catch (Exception ex)
        {
            lock (_lock)
            {
                _interaction.WriteLine($"Error processing '{folder.Name}' {ex.Message}");
            }
        }
    }

    private BookRole GetRole(string bookTitle, string person)
    {
        var answer = string.Empty;

        lock (_lock)
        {
            while (answer is not "a" and not "n" and not "b" and not "s")
            {
                _interaction.Write($"Is {person} (a)uthor, (n)arrator, (b)oth or (s)kip for '{bookTitle}'? ");

                answer = _interaction.ReadLine();
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

    private string GetName(string bookTitle, string role)
    {
        var answer = string.Empty;

        lock (_lock)
        {
            _interaction.Write($"No {role} found for '{bookTitle}'. Please enter {role}: ");

            answer = _interaction.ReadLine();
        }

        return answer;
    }

    private void Log(string message)
    {
        lock (_lock)
        {
            _interaction.WriteLine(message);
        }
    }
}
