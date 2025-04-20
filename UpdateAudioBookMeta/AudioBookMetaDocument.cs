using System.Xml.Serialization;
using DoenaSoft.MediaInfoHelper.DataObjects.AudioBookMetaXml;

namespace DoenaSoft.UpdateAudioBookMeta;

[XmlRoot("doc")]
public sealed class AudioBookMetaDocument
{
    public AudioBookMeta Mp3Meta;
}
