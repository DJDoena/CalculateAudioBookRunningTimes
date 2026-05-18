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

    public (char key, KeyModifiers modifiers) ReadKey(bool intercept)
    {
        var key = Console.ReadKey(intercept);

        return (key.KeyChar, (KeyModifiers)key.Modifiers);
    }
}