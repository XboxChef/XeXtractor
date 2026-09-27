using System;
using System.IO;
using System.Text;

namespace XeXtractor
{
    // Entry names come from the file being parsed, so they must never be trusted as paths.
    public static class SafePath
    {
        private static readonly char[] InvalidChars = BuildInvalidChars();

        private static readonly string[] ReservedNames =
        {
            "CON", "PRN", "AUX", "NUL",
            "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
            "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
        };

        private static char[] BuildInvalidChars()
        {
            StringBuilder chars = new StringBuilder("<>:\"/\\|?*");
            for (char c = '\0'; c < ' '; c++)
                chars.Append(c);
            chars.Append(Path.GetInvalidFileNameChars());
            return chars.ToString().ToCharArray();
        }

        // Turns an untrusted name into a single, valid file or folder name (no separators, no "..").
        public static string SanitizeName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "_";

            StringBuilder result = new StringBuilder(name.Length);
            foreach (char c in name)
                result.Append(Array.IndexOf(InvalidChars, c) >= 0 ? '_' : c);

            // Windows silently strips trailing dots and spaces, which would turn ".." into "".
            string sanitized = result.ToString().TrimEnd('.', ' ');
            if (sanitized.Trim().Length == 0)
                return "_";

            string baseName = sanitized.Split('.')[0].Trim();
            foreach (string reserved in ReservedNames)
            {
                if (string.Equals(baseName, reserved, StringComparison.OrdinalIgnoreCase))
                    return "_" + sanitized;
            }
            return sanitized;
        }

        // Combines a trusted root folder with an untrusted name and verifies the result stays inside the root.
        public static string Combine(string root, string untrustedName)
        {
            string fullRoot = Path.GetFullPath(root);
            string fullPath = Path.GetFullPath(Path.Combine(fullRoot, SanitizeName(untrustedName)));

            string rootWithSeparator = fullRoot.EndsWith(Path.DirectorySeparatorChar.ToString())
                ? fullRoot
                : fullRoot + Path.DirectorySeparatorChar;
            if (!fullPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
                throw new IOException("Refusing to write outside the target folder: " + untrustedName);

            return fullPath;
        }
    }
}
