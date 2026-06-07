using System;
using System.IO;
using System.Threading;

namespace XeXtractor
{
    public class FileHandler
    {
        public static event EventHandler ParseCompleted;

        public static void HandleFile(string fileName)
        {
            new Thread(HandleFileThreaded).Start(fileName);
        }

        private static void HandleFileThreaded(object fileName)
        {
            string path = fileName.ToString();
            byte[] data = File.ReadAllBytes(path);
            HandleFile(new FileEntry
            {
                Data = data,
                fileName = Path.GetFileNameWithoutExtension(path)
            });
            if (ParseCompleted != null)
                ParseCompleted(null, EventArgs.Empty);
        }

        public static string GetFileType(byte[] data)
        {
            EndianIo io = new EndianIo(data, EndianType.BigEndian);
            try
            {
                io.Open();
                return io.In.ReadAsciiString(4);
            }
            finally
            {
                io.Dispose();
            }
        }

        public static void HandleFile(FileEntry data)
        {
            try
            {
                if (data.Data == null || data.Data.Length <= 4)
                    return;

                string fileType = GetFileType(data.Data);
                switch (fileType)
                {
                    case "XEX2":
                        HandleXEX2(data.Data, Path.GetFileNameWithoutExtension(data.fileName));
                        break;
                    case "XSTR":
                        HandleXSTR(data.Data, Path.GetFileNameWithoutExtension(data.fileName));
                        break;
                    case "XSRC":
                        HandleXSRC(data.Data);
                        break;
                    case "XDBF":
                        HandleXDBF(data.Data);
                        break;
                    case "XUIZ":
                        HandleXUIZ(data.Data, data.fileName);
                        break;
                    default:
                        Log.getInstance().AddEntry("Unsupported file type: " + fileType);
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.getInstance().AddEntry("Error: " + ex.Message);
            }
        }

        private static void HandleXUIZ(byte[] file, string filename)
        {
            XUIZ xuiz = new XUIZ(file, filename);
            xuiz.Open();
            xuiz.Read();
            xuiz.Close();
        }

        private static void HandleXDBF(byte[] file)
        {
            XDBF xdbf = new XDBF(file);
            xdbf.Open();
            xdbf.Read();
            xdbf.ExtractEntryData();
            xdbf.Close();

            FileEntry[] binFiles = InnerFileStructure.getInstance().getFiles("XDBF", "Bins");
            foreach (FileEntry binFile in binFiles)
            {
                if (!binFile.fileName.ToLower().EndsWith("xitb"))
                    continue;

                XITB xitb = new XITB(binFile.Data);
                xitb.Open();
                xitb.Read();
                xitb.Close();
            }

            foreach (FileEntry binFile in binFiles)
            {
                if (!binFile.fileName.ToLower().EndsWith("xach"))
                    continue;

                FileEntry[] stringFiles = InnerFileStructure.getInstance().getFiles("XDBF", "Strings");
                foreach (FileEntry stringFile in stringFiles)
                {
                    XSTR xstr = new XSTR(stringFile.Data);
                    xstr.Open();
                    xstr.Read(true, "");
                    XACH xach = new XACH(binFile.Data);
                    xach.Open();
                    xach.Read(xstr, stringFile.fileName);
                    xach.Close();
                    xstr.Close();
                }
            }
        }

        private static void HandleXEX2(byte[] file, string fileName)
        {
            XEX2 xex = new XEX2(file);
            xex.Open();
            xex.Read();
            xex.DecryptBaseFile();
            InnerFileStructure.getInstance().AddFileEntry(new FileEntry
            {
                Data = xex.getBaseFile(),
                type = "Base File",
                fileName = fileName + ".exe"
            });
            xex.ExtractAllRessource();
            xex.Close();
        }

        private static void HandleXSRC(byte[] file)
        {
            XSRC xsrc = new XSRC(file);
            xsrc.Open();
            xsrc.Read();
            xsrc.Close();
        }

        private static void HandleXSTR(byte[] file, string sourceName)
        {
            XSTR xstr = new XSTR(file);
            xstr.Open();
            xstr.Read(false, sourceName);
            xstr.Close();
        }
    }
}
