using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CalculateAudioBookRunningTimes
{
    public static class Program
    {
        private static bool _reboot;

        private static readonly XmlSerializer _serializer = new XmlSerializer(typeof(Mp3Meta));

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Invalid parameter count.");

                return;
            }

            _reboot = false;

            if (args.Length == 3)
            {
                if (args[2].ToLower() == "/r")
                {
                    _reboot = true;
                }
                else
                {
                    Console.WriteLine($"Invalid parameter '{args[2]}'. Should be '/r'.");

                    return;
                }
            }

            var di = new DirectoryInfo(args[0]);

            if (!di.Exists)
            {
                Console.WriteLine($"'{di.FullName}' is not a valid directory.");

                return;
            }

            if (args.Length == 1)
            {
                ProcessBook(di);
            }
            else if (args[1].ToLower() == "/m")
            {
                ProcessBooks(di);
            }
            else
            {
                Console.WriteLine($"Invalid parameter '{args[1]}'. Should be '/m'.");

                return;
            }
        }

        private static void ProcessBooks(DirectoryInfo rootDI)
        {
            var dis = rootDI.GetDirectories("*.*", SearchOption.TopDirectoryOnly);

            foreach (var di in dis)
            {
                if (rootDI.Name == "English" || rootDI.Name == "Deutsch")
                {
                    ProcessBooks(di);
                }
                else
                {
                    ProcessBook(di);
                }
            }
        }

        private static void ProcessBook(DirectoryInfo di)
        {
            var outFileName = Path.Combine(di.FullName, $"{di.Name}.xml");

            if (_reboot && File.Exists(outFileName))
            {
                //UpdateXml(di, outFileName);

                return;
            }

            Console.WriteLine($"Processing '{di.Name}'.");

            var length = GetLength(di);

            CreateXml(di, length, outFileName);
        }

        private static (ushort hours, ushort minutes, ushort seconds) GetLength(DirectoryInfo di)
        {
            var fis = di.GetFiles("*.mp3", SearchOption.AllDirectories);

            var totalLength = new TimeSpan(0);

            foreach (var fi in fis.OrderBy(fi => fi.FullName))
            {
                Console.WriteLine($"Processing '{fi.Name}'.");

                using (var reader = new NAudio.Wave.Mp3FileReader(fi.FullName))
                {
                    totalLength += reader.TotalTime;
                }
            }

            var length = GetLength(totalLength);

            return length;
        }

        private static (ushort hours, ushort minutes, ushort seconds) GetLength(TimeSpan totalLength)
        {
            var days = totalLength.Days;

            var hours = totalLength.Hours;

            var minutes = totalLength.Minutes;

            var seconds = totalLength.Seconds;

            if (totalLength.Milliseconds >= 500)
            {
                seconds++;
            }

            if (seconds == 60)
            {
                seconds = 0;

                minutes++;
            }

            if (minutes == 60)
            {
                minutes = 0;

                hours++;
            }

            if (hours == 24)
            {
                hours = 0;

                days++;
            }

            if (days > 0)
            {
                hours += days * 24;
            }

            return ((ushort)hours, (ushort)minutes, (ushort)seconds);
        }

        private static void CreateXml(DirectoryInfo di, (ushort hours, ushort minutes, ushort seconds) length, string outFileName)
        {
            var fi = di.GetFiles("*.mp3", SearchOption.AllDirectories).OrderBy(f => f.FullName).FirstOrDefault();

            if (fi == null)
            {
                return;
            }

            var mp3Meta = GetTagMeta(fi, length);

            WriteXml(outFileName, mp3Meta);
        }

        private static Mp3Meta GetTagMeta(FileInfo fi, (ushort hours, ushort minutes, ushort seconds) length)
        {
            using (var fileMeta = TagLib.File.Create(fi.FullName))
            {
                var tag = fileMeta?.Tag;

                var meta = new Mp3Meta();

                if (tag != null)
                {
                    meta.Title = tag.Album;
                    meta.Author = tag.AlbumArtists?.Select(a => a).ToArray();
                    meta.Narrator = tag.Performers?.Select(p => p).ToArray();
                    meta.Genre = tag.Genres?.Select(g => g).ToArray();
                    meta.Description = GetDescription(tag);
                }

                meta.RunningTime = new RunningTime()
                {
                    Hours = length.hours,
                    Minutes = length.minutes,
                    Seconds = length.seconds,
                    Value = $"{length.hours}:{length.minutes:D2}:{length.seconds:D2}",
                };

                return meta;
            }
        }

        private static string GetDescription(TagLib.Tag tag)
        {
            var result = tag is TagLib.NonContainer.Tag nct
                ? GetDescription(nct)
                : tag.Comment;

            return result?.Trim();
        }

        private static string GetDescription(TagLib.NonContainer.Tag tag)
        {
            var texts = tag.Tags?.Where(t => t != null).OfType<TagLib.Id3v2.Tag>().SelectMany(t => t.OfType<TagLib.Id3v2.TextInformationFrame>()).ToList() ?? Enumerable.Empty<TagLib.Id3v2.TextInformationFrame>();

            var title3 = texts.FirstOrDefault(t => t.FrameId == "TIT3");

            if (title3?.Text.Length > 0)
            {
                var sb = new StringBuilder();

                foreach (var text in title3.Text)
                {
                    sb.AppendLine(text);
                }

                return sb.ToString();
            }
            else
            {
                return tag.Comment;
            }
        }

        private static void WriteXml(string outFileName, Mp3Meta mp3Meta)
        {
            using (var fs = new FileStream(outFileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (var xtw = new XmlTextWriter(fs, Encoding.UTF8))
                {
                    xtw.Formatting = Formatting.Indented;
                    xtw.Namespaces = false;

                    var ns = new XmlSerializerNamespaces();

                    ns.Add(string.Empty, string.Empty);

                    _serializer.Serialize(xtw, mp3Meta, ns);
                }
            }
        }

        private static void UpdateXml(DirectoryInfo di, string outFileName)
        {
            var fi = di.GetFiles("*.mp3", SearchOption.AllDirectories).OrderBy(f => f.FullName).FirstOrDefault();

            if (fi == null)
            {
                return;
            }

            Mp3Meta existingMeta;
            using (var fs = new FileStream(outFileName, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                existingMeta = (Mp3Meta)_serializer.Deserialize(fs);
            }

            var newMeta = GetTagMeta(fi, (existingMeta.RunningTime.Hours, existingMeta.RunningTime.Minutes, existingMeta.RunningTime.Seconds));

            newMeta.Title = existingMeta.Title;
            newMeta.Description = existingMeta.Description;

            if (newMeta.Author != null && newMeta.Author.Length > 0
                && newMeta.Narrator != null && newMeta.Narrator.Length > 0
                && newMeta.Author[0] == newMeta.Narrator[0])
            {
                Console.WriteLine($"Use narrator '{newMeta.Narrator[0]}' for title '{newMeta.Title}'?");

                if (Console.ReadKey().Key != ConsoleKey.Y)
                {
                    newMeta.Narrator = null;
                }
            }

            WriteXml(outFileName, newMeta);
        }
    }
}