using System;
using Tides.Core;

namespace Tides.Objects
{
    public static class Common
    {
        public class Article : BaseObject
        {
            public Article(string type = null) : base(type ?? nameof(Article)) { }
        }

        public class Document : BaseObject
        {
            public Document(string type = null) : base(type ?? nameof(Document)) { }
        }

        public class Audio : Document
        {
            public Audio(string type = null) : base(type ?? nameof(Audio)) { }
        }

        public class Image : Document
        {
            public Image(string type = null) : base(type ?? nameof(Image)) { }
        }

        public class Video : Document
        {
            public Video(string type = null) : base(type ?? nameof(Video)) { }
        }

        public class Note : BaseObject
        {
            public Note(string type = null) : base(type ?? nameof(Note)) { }
        }

        public class Page : Document
        {
            public Page(string type = null) : base(type ?? nameof(Page)) { }
        }

        public class Event : BaseObject
        {
            public Event(string type = null) : base(type ?? nameof(Event)) { }
        }

        public class Mention : LinkObject
        {
            public Mention(string type = null) : base(type ?? nameof(Mention)) { }
        }

        public class Profile : BaseObject
        {
            public Profile(string type = null) : base(type ?? nameof(Profile)) { }
            
            public BaseObject describes { get; set; }
        }

        public class Tombstone : BaseObject
        {
            public Tombstone(string type = null) : base(type ?? nameof(Tombstone)) { }

            public string formerType { get; set; }
            
            public DateTime deleted { get; set; }
        }
    }
}