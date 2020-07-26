using System.Collections.Generic;

namespace Tides.Streams
{
    public class FolderObjectStreamReader : ChunkedObjectStreamReader<string, ObjectStreamReader>
    {
        public FolderObjectStreamReader(string directory) : base(GetPaths(directory), StreamLoader)
        {
        }

        private static IEnumerable<string> GetPaths(string directory) => System.IO.Directory.EnumerateFiles(directory);

        private static ObjectStreamReader StreamLoader(string path) => new ObjectStreamReader(System.IO.File.OpenRead(path));
    }
}