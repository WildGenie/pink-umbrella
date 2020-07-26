using Tides.Core;
using Tides.Util;

namespace Tides.Activities
{
    public static class Common
    {
        [IsDocumented]
        public class Accept : ActivityObject
        {
            public Accept(string type = null) : base(type ?? nameof(Accept)) { }
        }

        [IsDocumented]
        public class Add : ActivityObject
        {
            public Add(string type = null) : base(type ?? nameof(Add)) { }
        }

        [IsDocumented]
        public class Announce : ActivityObject
        {
            public Announce(string type = null) : base(type ?? nameof(Announce)) { }
        }

        [IsDocumented]
        public class Arrive : ActivityObject
        {
            public Arrive(string type = null) : base(type ?? nameof(Arrive)) { }
        }

        [IsDocumented]
        public class Block : Ignore
        {
            public Block(string type = null) : base(type ?? nameof(Block)) { }
        }

        [IsDocumented]
        public class Report : Ignore
        {
            public Report(string type = null) : base(type ?? nameof(Report)) { }
        }

        [IsDocumented]
        public class Create : ActivityObject
        {
            public Create(string type = null) : base(type ?? nameof(Create)) { }
        }

        [IsDocumented]
        public class Delete : ActivityObject
        {
            public Delete(string type = null) : base(type ?? nameof(Delete)) { }
        }

        [IsDocumented]
        public class Dislike : ActivityObject
        {
            public Dislike(string type = null) : base(type ?? nameof(Dislike)) { }
        }

        [IsDocumented]
        public class Flag : ActivityObject
        {
            public Flag(string type = null) : base(type ?? nameof(Flag)) { }
        }

        [IsDocumented]
        public class Follow : ActivityObject
        {
            public Follow(string type = null) : base(type ?? nameof(Follow)) { }
        }

        [IsDocumented]
        public class Ignore : ActivityObject
        {
            public Ignore(string type = null) : base(type ?? nameof(Ignore)) { }
        }

        [IsDocumented]
        public class Invite : Offer
        {
            public Invite(string type = null) : base(type ?? nameof(Invite)) { }
        }

        [IsDocumented]
        public class Join : ActivityObject
        {
            public Join(string type = null) : base(type ?? nameof(Join)) { }
        }

        [IsDocumented]
        public class Leave : ActivityObject
        {
            public Leave(string type = null) : base(type ?? nameof(Leave)) { }
        }

        [IsDocumented]
        public class Like : ActivityObject
        {
            public Like(string type = null) : base(type ?? nameof(Like)) { }
        }

        [IsDocumented]
        public class Listen : ActivityObject
        {
            public Listen(string type = null) : base(type ?? nameof(Listen)) { }
        }

        [IsDocumented]
        public class Move : ActivityObject
        {
            public Move(string type = null) : base(type ?? nameof(Move)) { }
        }

        [IsDocumented]
        public class Offer : ActivityObject
        {
            public Offer(string type = null) : base(type ?? nameof(Offer)) { }
        }

        [IsDocumented]
        public class Reject : ActivityObject
        {
            public Reject(string type = null) : base(type ?? nameof(Reject)) { }
        }

        [IsDocumented]
        public class Read : ActivityObject
        {
            public Read(string type = null) : base(type ?? nameof(Read)) { }
        }

        [IsDocumented]
        public class Remove : ActivityObject
        {
            public Remove(string type = null) : base(type ?? nameof(Remove)) { }
        }

        [IsDocumented]
        public class TentativeReject : Reject
        {
            public TentativeReject(string type = null) : base(type ?? nameof(TentativeReject)) { }
        }

        [IsDocumented]
        public class TentativeAccept : Accept
        {
            public TentativeAccept(string type = null) : base(type ?? nameof(TentativeAccept)) { }
        }

        [IsDocumented]
        public class Travel : ActivityObject
        {
            public Travel(string type = null) : base(type ?? nameof(Travel)) { }
        }

        [IsDocumented]
        public class Undo : ActivityObject
        {
            public Undo(string type = null) : base(type ?? nameof(Undo)) { }
        }

        [IsDocumented]
        public class Update : ActivityObject
        {
            public Update(string type = null) : base(type ?? nameof(Update)) { }
        }

        [IsDocumented]
        public class View : ActivityObject
        {
            public View(string type = null) : base(type ?? nameof(View)) { }
        }
    }
}