using System;
using System.Collections.Generic;

namespace XeXtractor
{
    public class InnerFileStructure
    {
        private readonly List<FileEntry> files = new List<FileEntry>();
        private static InnerFileStructure instance;

        public event EventHandler FileAdded;

        private InnerFileStructure()
        {
        }

        public FileEntry[] getFiles() => files.ToArray();

        public FileEntry[] getFiles(string type, string folder)
        {
            List<FileEntry> matches = new List<FileEntry>();
            foreach (FileEntry file in files)
            {
                if (file.type == type && file.folder == folder)
                    matches.Add(file);
            }
            return matches.ToArray();
        }

        public FileEntry[] getFiles(string type, string folder, long id, bool exactFolder)
        {
            List<FileEntry> matches = new List<FileEntry>();
            foreach (FileEntry file in files)
            {
                if (file.type != type || file.id != id)
                    continue;

                bool include = exactFolder
                    ? file.folder == folder
                    : file.folder.StartsWith(folder);

                if (include)
                    matches.Add(file);
            }
            return matches.ToArray();
        }

        public void AddFileEntry(FileEntry entry)
        {
            files.Add(entry);
            FileHandler.HandleFile(entry);
            if (FileAdded != null)
                FileAdded(this, EventArgs.Empty);
        }

        public void Clear()
        {
            files.Clear();
            if (FileAdded != null)
                FileAdded(this, EventArgs.Empty);
        }

        public static InnerFileStructure getInstance()
        {
            if (instance == null)
                instance = new InnerFileStructure();
            return instance;
        }
    }
}
