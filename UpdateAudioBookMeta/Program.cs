using DoenaSoft.CalculateAudioBookRunningTimes;
using DoenaSoft.MediaInfoHelper.DataObjects.AudioBookMetaXml;
using DoenaSoft.ToolBox.Generics;
using DoenaSoft.UpdateAudioBookMeta;

var fileNames = Directory.GetFiles(@"N:\Drive3\AudioBooks", "*.xml", SearchOption.AllDirectories);

foreach (var fileName in fileNames)
{
    Console.WriteLine($"v {typeof(Program).Assembly.GetName().Version}");

    if (fileName == @"N:\Drive3\AudioBooks\audiobooks.xml")
    {
        continue;
    }

    AudioBookMeta mp3Meta;
    try
    {
        mp3Meta = XmlSerializer<AudioBookMeta>.Deserialize(fileName);
    }
    catch
    {
        try
        {
            var mp3MetaDocument = XmlSerializer<AudioBookMetaDocument>.Deserialize(fileName);

            mp3Meta = mp3MetaDocument.Mp3Meta;
        }
        catch
        {
            Console.WriteLine($"Error reading file '{fileName}'");

            continue;
        }
    }

    (new XsltSerializer<AudioBookMeta>(new RootItemXsltSerializerDataProvider())).Serialize(fileName, mp3Meta);
}

Console.WriteLine("Press <enter> to exit.");
Console.ReadLine();