namespace DoenaSoft.CalculateAudioBookRunningTimes;

public interface IInteraction
{
    void WriteLine(string message = null);

    void Write(string message);

    string ReadLine();
}