namespace Tides.Actors
{
    public static class Common
    {
        public class Application : ActorObject
        {
            public Application(string type = null) : base(type ?? nameof(Application)) { }
        }

        public class Group : ActorObject
        {
            public Group(string type = null) : base(type ?? nameof(Group)) { }
        }

        public class Organization : ActorObject
        {
            public Organization(string type = null) : base(type ?? nameof(Organization)) { }
        }

        public class Person : ActorObject
        {
            public Person(string type = null) : base(type ?? nameof(Person)) { }
        }

        public class Service : ActorObject
        {
            public Service(string type = null) : base(type ?? nameof(Service)) { }
        }
    }
}