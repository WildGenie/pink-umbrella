using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Tides.Streams
{
    public class FolderObjectStreamWriter : ChunkedObjectStreamWriter<string, ObjectStreamWriter>
    {
        public FolderObjectStreamWriter(string directory) : base(directory, StreamLoader)
        {
        }

        public override string Progress(string path)
        {
            if (System.IO.Directory.Exists(path))
            {
                var existing = System.IO.Directory.EnumerateFiles(path).LastOrDefault();
                if (existing != null)
                {
                    using (var reader = new StreamReader(System.IO.File.OpenRead(existing)))
                    {
                        var props = (ChunkedObjectstreamProperties) System.Text.Json.JsonSerializer.Deserialize(reader.ReadLine(), typeof(ChunkedObjectstreamProperties));
                        if (props.count < _chunkSize)
                        {
                            return existing;
                        }
                    }
                }
            }
            else
            {
                System.IO.Directory.CreateDirectory(path);
            }

            var next = $"{path}/{System.IO.Directory.EnumerateFiles(path).Count()}.chunk.activity-stream";
            File.WriteAllText(next, System.Text.Json.JsonSerializer.Serialize(new ChunkedObjectstreamProperties()));
            return next;
        }

        private static IEnumerable<string> GetPaths(string directory) => System.IO.Directory.EnumerateFiles(directory);

        private static ObjectStreamWriter StreamLoader(string path) => new ObjectStreamWriter(System.IO.File.OpenRead(path));
    }
}