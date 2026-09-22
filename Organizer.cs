using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MusicFilesOrganizer
{
    public class OrganizeOptions
    {
        public List<MetadataToken> FolderTokens { get; set; } = new List<MetadataToken>();
        public List<MetadataToken> FileNameTokens { get; set; } = new List<MetadataToken>();
        public string FileNameSeparator { get; set; } = " - ";
        public string DestinationRoot { get; set; } = "";
        public bool CopyInsteadOfMove { get; set; } = false;
    }

    public class OrganizeResult
    {
        public string SourcePath { get; set; } = "";
        public string DestinationPath { get; set; } = "";
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }

    /// <summary>
    /// Replaces characters that Windows does not allow in file/folder names
    /// (this also takes care of things like "AC/DC" as an artist name,
    /// which would otherwise be read as a sub-folder separator).
    /// </summary>
    public static class PathSanitizer
    {
        private static readonly char[] InvalidChars = Path.GetInvalidFileNameChars();

        public static string SanitizeSegment(string segment)
        {
            if (string.IsNullOrWhiteSpace(segment)) return "Unknown";

            var sb = new StringBuilder(segment.Length);
            foreach (char c in segment)
            {
                sb.Append(Array.IndexOf(InvalidChars, c) >= 0 ? '_' : c);
            }

            string result = sb.ToString().Trim().TrimEnd('.');
            return string.IsNullOrWhiteSpace(result) ? "Unknown" : result;
        }
    }

    /// <summary>
    /// Builds destination paths from the chosen tag tokens and performs the
    /// actual move/copy of files on disk.
    /// </summary>
    public static class Organizer
    {
        /// <summary>
        /// Builds the path of the output file, relative to the destination
        /// root, from the ordered folder tokens (each token becomes one
        /// nested sub-folder) and the ordered file name tokens (joined with
        /// the chosen separator).
        /// </summary>
        public static string BuildRelativePath(TagInfo info, OrganizeOptions options)
        {
            var folderParts = options.FolderTokens
                .Select(t => PathSanitizer.SanitizeSegment(TokenResolver.Resolve(t, info)))
                .ToArray();

            var nameParts = options.FileNameTokens
                .Select(t => PathSanitizer.SanitizeSegment(TokenResolver.Resolve(t, info)))
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToArray();

            string separator = options.FileNameSeparator ?? " - ";
            string fileName = string.Join(separator, nameParts);

            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = Path.GetFileNameWithoutExtension(info.SourcePath);
            }

            fileName = PathSanitizer.SanitizeSegment(fileName) + info.Extension;

            string relativeDir = folderParts.Length > 0 ? Path.Combine(folderParts) : "";
            return string.IsNullOrEmpty(relativeDir) ? fileName : Path.Combine(relativeDir, fileName);
        }

        /// <summary>
        /// Reads the tags of one file, works out its destination path and
        /// moves (or copies) it there, creating folders as needed and
        /// avoiding overwrites by appending " (1)", " (2)", etc.
        /// </summary>
        public static OrganizeResult ProcessFile(string sourcePath, OrganizeOptions options)
        {
            var result = new OrganizeResult { SourcePath = sourcePath };

            TagInfo info = TagReader.ReadTags(sourcePath);
            if (info == null)
            {
                result.Success = false;
                result.Message = "Could not read tags from this file.";
                return result;
            }

            try
            {
                string relative = BuildRelativePath(info, options);
                string destPath = Path.Combine(options.DestinationRoot, relative);
                string destDir = Path.GetDirectoryName(destPath);

                if (!string.IsNullOrEmpty(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                destPath = MakeUnique(destPath);

                if (options.CopyInsteadOfMove)
                {
                    File.Copy(sourcePath, destPath, false);
                }
                else
                {
                    File.Move(sourcePath, destPath);
                }

                result.DestinationPath = destPath;
                result.Success = true;
                result.Message = "OK";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        private static string MakeUnique(string path)
        {
            if (!File.Exists(path)) return path;

            string dir = Path.GetDirectoryName(path);
            string name = Path.GetFileNameWithoutExtension(path);
            string ext = Path.GetExtension(path);

            int counter = 1;
            string candidate;
            do
            {
                candidate = Path.Combine(dir, string.Format("{0} ({1}){2}", name, counter, ext));
                counter++;
            }
            while (File.Exists(candidate));

            return candidate;
        }
    }
}
