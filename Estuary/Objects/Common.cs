using Estuary.Core;

namespace Estuary.Objects
{
    public static class Common
    {
        public class Article : BaseObject
        {
            public Article(string type, string baseType) : base(type ?? nameof(Article), baseType) { }

            public Article(): this(null, null) { }
        }

        public class Document : BaseObject
        {
            public Document(string type, string baseType) : base(type ?? nameof(Document), baseType) { }

            public Document(): this(null, null) { }
        }

        public class Audio : Document
        {
            public Audio(string type) : base(type ?? nameof(Audio), nameof(Document)) { }

            public Audio(): this(null) { }
        }

        public class Image : Document
        {
            public Image(string type) : base(type ?? nameof(Image), nameof(Document)) { }

            public Image(): this(null) { }
        }

        public class Video : Document
        {
            public Video(string type) : base(type ?? nameof(Video), nameof(Document)) { }

            public Video(): this(null) { }
        }

        public class Note : BaseObject
        {
            public Note(string type) : base(type ?? nameof(Note), null) { }

            public Note(): this(null) { }
        }

        public class Page : Document
        {
            public Page(string type) : base(type ?? nameof(Page), nameof(Document)) { }

            public Page(): this(null) { }
        }

        public class Event : BaseObject
        {
            public Event(string type) : base(type ?? nameof(Event), null) { }

            public Event(): this(null) { }
        }

        public class Mention : Link
        {
            public Mention(string type) : base(type ?? nameof(Mention), nameof(Link)) { }

            public Mention(): this(null) { }
        }

        public class Profile : BaseObject
        {
            public Profile(string type) : base(type ?? nameof(Profile), null) { }

            public Profile(): this(null) { }
            
            public BaseObject describes { get; set; }
        }

        public class Tombstone : BaseObject
        {
            public Tombstone(string type) : base(type ?? nameof(Tombstone), null) { }

            public Tombstone(): this(null) { }

            public string formerType { get; set; }
        }
    }
}