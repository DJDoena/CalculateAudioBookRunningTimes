using DoenaSoft.AbstractionLayer.UI.Contracts;

namespace DoenaSoft.CalculateAudioBookRunningTimes;

internal sealed class Interaction : IInteraction
{
    public string ReadLine()
        => Console.ReadLine();

    public void Write(string message)
        => Console.Write(message);

    public void WriteLine(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            Console.WriteLine();
        }
        else
        {
            Console.WriteLine(message);
        }
    }

    public char ReadKey(bool intercept = false)
        => Console.ReadKey(intercept).KeyChar;
}