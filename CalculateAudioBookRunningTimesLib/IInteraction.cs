namespace DoenaSoft.CalculateAudioBookRunningTimes;

/// <summary>
/// Defines methods for user interaction during audiobook processing.
/// </summary>
public interface IInteraction
{
    /// <summary>
    /// Writes a line of text to the output.
    /// </summary>
    /// <param name="message">The message to write. If null, writes an empty line.</param>
    void WriteLine(string message = null);

    /// <summary>
    /// Writes text to the output without a newline.
    /// </summary>
    /// <param name="message">The message to write.</param>
    void Write(string message);

    /// <summary>
    /// Reads a line of text from the input.
    /// </summary>
    /// <returns>The line of text read from the input.</returns>
    string ReadLine();
}