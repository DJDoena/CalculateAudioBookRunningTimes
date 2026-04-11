using DoenaSoft.AbstractionLayer.IOServices;

namespace DoenaSoft.CalculateAudioBookRunningTimes;

public static class Program
{
    public static void Main(string[] args)
    {
        var interaction = new Interaction();

        interaction.WriteLine($"v{typeof(Program).Assembly.GetName().Version}");

        if (args.Length == 0)
        {
            interaction.WriteLine("Invalid parameter count.");

            return;
        }

        var ioServices = new IOServices();

        string path;
        if (args.Any(a => a == "/getpath"))
        {
            do
            {
                interaction.WriteLine("Enter path:");

                path = interaction.ReadLine().Trim().Trim('"');

            } while (!ioServices.Folder.Exists(path));
        }
        else
        {
            path = args[0];
        }

        var reboot = args.Any(a => a == "/r");

        var mp4 = args.Any(a => a == "/mp4");

        var folder = ioServices.GetFolder(path);

        if (!folder.Exists)
        {
            interaction.WriteLine($"'{folder.FullName}' is not a valid directory.");

            return;
        }

        var bookProcessor = new BookProcessor(reboot, mp4, interaction);

        if (args.Any(a => a == "/m"))
        {
            (new BooksProcessor(bookProcessor)).Process(folder);
        }
        else
        {
            bookProcessor.Process(folder);
        }
    }
}