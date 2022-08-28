#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using M3U.NET.Extensions;

#endregion

namespace M3U.NET
{
    public class M3UFile : ICollection<M3UEntry>
    {
        private readonly List<M3UEntry> _entries = new List<M3UEntry>();

        public M3UEntry this[int index]
        {
            get { return _entries[index]; }
        }

        public void Load(string fileName, bool resolveRelativePaths = false)
        {
            _entries.Clear();

            using (var reader = new StreamReader(fileName))
            {
                var workingUri = new Uri(Path.GetDirectoryName(fileName));

                string line;
                var lineCount = 0;

                M3UEntry entry = null;

                while ((line = reader.ReadLine()) != null)
                {
                    if (lineCount == 0 && line != "#EXTM3U")
                        throw new M3UException("M3U header is missing.");

                    if (line.StartsWith("#EXTINF:"))
                    {
                        if (entry != null)
                            throw new M3UException("Unexpected entry detected.");

                        var split = line.Substring(8, line.Length - 8).Split(new[] {','}, 2);

                        if (split.Length != 2)
                            throw new M3UException("Invalid track information.");

                        int seconds;
                        if (!int.TryParse(split[0], out seconds))
                            throw new M3UException("Invalid track duration.");
                           
                        var title = split[1];

                        var duration = TimeSpan.FromSeconds(seconds);

                        entry = new M3UEntry(duration, title, null);
                    }

                    else if (entry != null && !line.StartsWith("#")) //ignore comments
                    {
                        Uri path;
                        if (!Uri.TryCreate(line, UriKind.RelativeOrAbsolute, out path))
                            throw new M3UException("Invalid entry path.");

                        if (path.IsFile && resolveRelativePaths)
                            path = path.MakeAbsoluteUri(workingUri);

                        entry.Path = path;

                        _entries.Add(entry);

                        entry = null;
                    }

                    lineCount++;
                }
            }
        }

        public void Save(string fileName, bool useAbsolutePaths = false, bool useLocalFilePath = true)
        {
            var workingUri = new Uri(Path.GetDirectoryName(fileName));

            using (var writer = new StreamWriter(fileName))
            {
                writer.WriteLine("#EXTM3U");

                foreach (var entry in this)
                {
                    writer.WriteLine("#EXTINF:{0},{1}", entry.Duration.TotalSeconds, entry.Title);

                    if (entry.Path.IsFile && useLocalFilePath)
                        writer.WriteLine(entry.Path.LocalPath);
                    else if (!entry.Path.IsAbsoluteUri && useAbsolutePaths)
                        writer.WriteLine(entry.Path.MakeAbsoluteUri(workingUri));
                    else
                        writer.WriteLine(entry.Path);
                }
            }
        }

        private bool FixFileName(ref string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return true;
            }
            string[] problemChar = new string[] { "【", "】"};
            string[] fixChar = new string[] { "[", "]"};
            string[] errChar = new string[] { "/", "\\", ":", ",", "*", "?", "\"", "<", ">", "|" };

            for (int i = 0; i < errChar.Length; i++)
            {
                fileName = fileName.Replace(errChar[i], "");
            }
            for (int i = 0; i < fixChar.Length; i++)
            {
                fileName = fileName.Replace(problemChar[i], fixChar[i]);
            }
            return false;
        } 

        public void SaveToFiles(string floder, bool useAbsolutePaths = false, bool useLocalFilePath = true)
        {
            var workingUri = new Uri(Path.GetDirectoryName(floder));

            foreach (var entry in this)
            {
                string fileName = entry.Title;
                if(FixFileName(ref fileName))
                {
                    continue;
                }
                if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                {
                    continue;
                }
                string filePath = Path.Combine(floder, entry.Title + ".m3u8");
                if(File.Exists(filePath))
                {
                    continue;
                }
                using (var writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("#EXTM3U");
                    writer.WriteLine("#EXTINF:{0},{1}", entry.Duration.TotalSeconds, entry.Title);

                    if (entry.Path.IsFile && useLocalFilePath)
                        writer.WriteLine(entry.Path.LocalPath);
                    else if (!entry.Path.IsAbsoluteUri && useAbsolutePaths)
                        writer.WriteLine(entry.Path.MakeAbsoluteUri(workingUri));
                    else
                        writer.WriteLine(entry.Path);
                }

            }
        }

        #region Implementation of IEnumerable

        public IEnumerator<M3UEntry> GetEnumerator()
        {
            return ((IEnumerable<M3UEntry>) _entries).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of ICollection<M3UEntry>

        public void Add(M3UEntry item)
        {
            _entries.Add(item);
        }

        public void Clear()
        {
            _entries.Clear();
        }

        public bool Contains(M3UEntry item)
        {
            return _entries.Contains(item);
        }

        public void CopyTo(M3UEntry[] array, int arrayIndex)
        {
            _entries.CopyTo(array, arrayIndex);
        }

        public bool Remove(M3UEntry item)
        {
            return _entries.Remove(item);
        }

        public int Count
        {
            get { return _entries.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        #endregion

        public M3UEntry Find(Predicate<M3UEntry> match)
        {
            return _entries.Find(match);
        }

        public List<M3UEntry> FindAll(Predicate<M3UEntry> match)
        {
            return _entries.FindAll(match);
        }
    }
}