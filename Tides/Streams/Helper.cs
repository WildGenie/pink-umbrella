using System;
using System.IO;

namespace Tides.Streams
{
    public static class Helper
    {
        private static readonly string PROTOCOL_SEP = "://";

        public static BaseObjectStreamReader OpenRead(string path)
        {
            var protocolSep = path.IndexOf(PROTOCOL_SEP);
            if (protocolSep > 0)
            {
                var newPath = path.Substring(protocolSep + PROTOCOL_SEP.Length);
                switch (path.Substring(0, protocolSep))
                {
                    case "file": return new ObjectStreamReader(File.OpenRead(newPath));
                    case "dir": return new FolderObjectStreamReader(newPath);
                    case "https": throw new NotImplementedException(); //return new ObjectStreamReader(File.OpenRead(newPath));
                }
            }
            throw new ArgumentException($"Invalid path {path}");
        }

        public static BaseObjectStreamWriter OpenWrite(string path)
        {
            var protocolSep = path.IndexOf(PROTOCOL_SEP);
            if (protocolSep > 0)
            {
                var newPath = path.Substring(protocolSep + PROTOCOL_SEP.Length);
                switch (path.Substring(0, protocolSep))
                {
                    case "file": return new ObjectStreamWriter(File.OpenRead(newPath));
                    case "dir": return new FolderObjectStreamWriter(newPath);
                    case "https": throw new NotImplementedException(); //return new ObjectStreamWriter(File.OpenRead(newPath));
                }
            }
            throw new ArgumentException($"Invalid path {path}");
        }
    }
}