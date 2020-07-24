using Tides.Core;

namespace Tides.Actors
{
    public static class Common
    {
        public class Application : BaseObject
        {
            public Application(string type = null) : base(type ?? nameof(Application)) { }
        }

        public class Group : BaseObject
        {
            public Group(string type = null) : base(type ?? nameof(Group)) { }
        }

        public class Organization : BaseObject
        {
            public Organization(string type = null) : base(type ?? nameof(Organization)) { }
        }

        public class Person : BaseObject
        {
            public Person(string type = null) : base(type ?? nameof(Person)) { }
        }

        public class Service : BaseObject
        {
            public Service(string type = null) : base(type ?? nameof(Service)) { }
        }
    }
}