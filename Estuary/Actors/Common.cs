namespace Estuary.Actors
{
    public static class Common
    {
        public class Application : ActorObject
        {
            public Application(string type) : base(type ?? nameof(Application)) { }
            public Application(): this(null) { }
        }

        public class Group : ActorObject
        {
            public Group(string type) : base(type ?? nameof(Group)) { }
            public Group(): this(null) { }
        }

        public class Organization : ActorObject
        {
            public Organization(string type) : base(type ?? nameof(Organization)) { }
            public Organization(): this(null) { }
        }

        public class Person : ActorObject
        {
            public Person(string type) : base(type ?? nameof(Person)) { }
            public Person(): this(null) { }
        }

        public class Service : ActorObject
        {
            public Service(string type) : base(type ?? nameof(Service)) { }
            public Service(): this(null) { }
        }
    }
}