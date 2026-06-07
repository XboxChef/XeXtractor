using System.IO;

namespace XeXtractor
{
    public class FileEntry
    {
        public string fileName = "";
        public string folder = "";
        public string type = "";
        public byte[] Data;
        public long id = -1;
        public int iconId;

        public override string ToString() => fileName;

        public void SaveAs(string file)
        {
            if (Data == null || Data.Length == 0 || type == "XACH")
                return;

            string directoryName = Path.GetDirectoryName(file);
            if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);

            File.WriteAllBytes(file, Data);
        }
    }
}
