using System;
using System.IO;

namespace MusicFilesOrganizer
{
    /// <summary>
    /// Wraps TagLib# to read metadata from a wide range of audio formats:
    /// MP3 (ID3v1/ID3v2), FLAC, Ogg Vorbis/Opus, MP4/M4A (AAC/ALAC), WMA,
    /// WAV, AIFF, APE, WavPack, Musepack, TTA, DSF, DFF and more.
    /// </summary>
    public static class TagReader
    {
        public static readonly string[] SupportedExtensions =
        {
            ".mp3", ".flac", ".m4a", ".m4b", ".mp4", ".aac",
            ".ogg", ".oga", ".opus", ".wma", ".wav", ".aiff", ".aif",
            ".ape", ".wv", ".mpc", ".tta", ".dsf", ".dff"
        };

        public static bool IsSupported(string path)
        {
            string ext = Path.GetExtension(path).ToLowerInvariant();
            foreach (var supported in SupportedExtensions)
            {
                if (supported == ext) return true;
            }
            return false;
        }

        /// <summary>
        /// Reads tags from a file. Returns null if the file could not be
        /// opened or parsed (corrupt file, unsupported codec, etc.) so the
        /// caller can skip it and report the failure.
        /// </summary>
        public static TagInfo ReadTags(string filePath)
        {
            try
            {
                using (TagLib.File file = TagLib.File.Create(filePath))
                {
                    TagLib.Tag tag = file.Tag;

                    string title = string.IsNullOrWhiteSpace(tag.Title)
                        ? Path.GetFileNameWithoutExtension(filePath)
                        : tag.Title.Trim();

                    string artist = FirstNonEmpty(tag.Performers, tag.AlbumArtists, "Unknown Artist");
                    string albumArtist = FirstNonEmpty(tag.AlbumArtists, tag.Performers, "Unknown Artist");
                    string genre = FirstNonEmpty(tag.Genres, Array.Empty<string>(), "Unknown Genre");
                    string album = string.IsNullOrWhiteSpace(tag.Album) ? "Unknown Album" : tag.Album.Trim();
                    string year = tag.Year > 0 ? tag.Year.ToString() : "Unknown Year";
                    string track = tag.Track > 0 ? tag.Track.ToString("D2") : "00";
                    string disc = tag.Disc > 0 ? tag.Disc.ToString() : "1";

                    return new TagInfo
                    {
                        Title = title,
                        Artist = artist,
                        AlbumArtist = albumArtist,
                        Album = album,
                        Year = year,
                        Genre = genre,
                        TrackNumber = track,
                        DiscNumber = disc,
                        Extension = Path.GetExtension(filePath),
                        SourcePath = filePath
                    };
                }
            }
            catch
            {
                return null;
            }
        }

        private static string FirstNonEmpty(string[] primary, string[] fallback, string defaultValue)
        {
            if (primary != null)
            {
                foreach (var value in primary)
                {
                    if (!string.IsNullOrWhiteSpace(value)) return value.Trim();
                }
            }
            if (fallback != null)
            {
                foreach (var value in fallback)
                {
                    if (!string.IsNullOrWhiteSpace(value)) return value.Trim();
                }
            }
            return defaultValue;
        }
    }
}
