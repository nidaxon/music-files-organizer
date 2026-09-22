using System;

namespace MusicFilesOrganizer
{
    /// <summary>
    /// Plain data holder for the tag values read from one audio file.
    /// </summary>
    public class TagInfo
    {
        public string Title { get; set; } = "Unknown Title";
        public string Artist { get; set; } = "Unknown Artist";
        public string AlbumArtist { get; set; } = "Unknown Artist";
        public string Album { get; set; } = "Unknown Album";
        public string Year { get; set; } = "Unknown Year";
        public string Genre { get; set; } = "Unknown Genre";
        public string TrackNumber { get; set; } = "00";
        public string DiscNumber { get; set; } = "1";
        public string Extension { get; set; } = "";
        public string SourcePath { get; set; } = "";
    }

    /// <summary>
    /// The tag fields the user can pick from when building the destination
    /// folder structure and the output file name.
    /// </summary>
    public enum MetadataToken
    {
        Artist,
        AlbumArtist,
        Album,
        Year,
        Genre,
        Title,
        TrackNumber,
        DiscNumber
    }

    /// <summary>
    /// Turns a MetadataToken into the actual string value for a given file,
    /// or into a friendly display label for the UI.
    /// </summary>
    public static class TokenResolver
    {
        public static string Resolve(MetadataToken token, TagInfo info)
        {
            switch (token)
            {
                case MetadataToken.Artist: return info.Artist;
                case MetadataToken.AlbumArtist: return info.AlbumArtist;
                case MetadataToken.Album: return info.Album;
                case MetadataToken.Year: return info.Year;
                case MetadataToken.Genre: return info.Genre;
                case MetadataToken.Title: return info.Title;
                case MetadataToken.TrackNumber: return info.TrackNumber;
                case MetadataToken.DiscNumber: return info.DiscNumber;
                default: return "";
            }
        }

        public static string Label(MetadataToken token)
        {
            switch (token)
            {
                case MetadataToken.Artist: return "Artist";
                case MetadataToken.AlbumArtist: return "Album Artist";
                case MetadataToken.Album: return "Album";
                case MetadataToken.Year: return "Year";
                case MetadataToken.Genre: return "Genre";
                case MetadataToken.Title: return "Title";
                case MetadataToken.TrackNumber: return "Track Number";
                case MetadataToken.DiscNumber: return "Disc Number";
                default: return token.ToString();
            }
        }
    }
}
