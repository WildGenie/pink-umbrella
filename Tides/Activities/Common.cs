using Tides.Core;

namespace Tides.Activities
{
    public static class Common
    {
        public class Accept : ActivityObject
        {
            public Accept(string type = null) : base(type ?? nameof(Accept)) { }
        }

        public class Add : ActivityObject
        {
            public Add(string type = null) : base(type ?? nameof(Add)) { }
        }

        public class Announce : ActivityObject
        {
            public Announce(string type = null) : base(type ?? nameof(Announce)) { }
        }

        public class Arrive : ActivityObject
        {
            public Arrive(string type = null) : base(type ?? nameof(Arrive)) { }
        }

        public class Block : Ignore
        {
            public Block(string type = null) : base(type ?? nameof(Block)) { }
        }

        public class Create : ActivityObject
        {
            public Create(string type = null) : base(type ?? nameof(Create)) { }
        }

        public class Delete : ActivityObject
        {
            public Delete(string type = null) : base(type ?? nameof(Delete)) { }
        }

        public class Dislike : ActivityObject
        {
            public Dislike(string type = null) : base(type ?? nameof(Dislike)) { }
        }

        public class Flag : ActivityObject
        {
            public Flag(string type = null) : base(type ?? nameof(Flag)) { }
        }

        public class Follow : ActivityObject
        {
            public Follow(string type = null) : base(type ?? nameof(Follow)) { }
        }

        public class Ignore : ActivityObject
        {
            public Ignore(string type = null) : base(type ?? nameof(Ignore)) { }
        }

        public class Invite : Offer
        {
            public Invite(string type = null) : base(type ?? nameof(Invite)) { }
        }

        public class Join : ActivityObject
        {
            public Join(string type = null) : base(type ?? nameof(Join)) { }
        }

        public class Leave : ActivityObject
        {
            public Leave(string type = null) : base(type ?? nameof(Leave)) { }
        }

        public class Like : ActivityObject
        {
            public Like(string type = null) : base(type ?? nameof(Like)) { }
        }

        public class Listen : ActivityObject
        {
            public Listen(string type = null) : base(type ?? nameof(Listen)) { }
        }

        public class Move : ActivityObject
        {
            public Move(string type = null) : base(type ?? nameof(Move)) { }
        }

        public class Offer : ActivityObject
        {
            public Offer(string type = null) : base(type ?? nameof(Offer)) { }
        }

        public class Reject : ActivityObject
        {
            public Reject(string type = null) : base(type ?? nameof(Reject)) { }
        }

        public class Read : ActivityObject
        {
            public Read(string type = null) : base(type ?? nameof(Read)) { }
        }

        public class Remove : ActivityObject
        {
            public Remove(string type = null) : base(type ?? nameof(Remove)) { }
        }

        public class TentativeReject : Reject
        {
            public TentativeReject(string type = null) : base(type ?? nameof(TentativeReject)) { }
        }

        public class TentativeAccept : Accept
        {
            public TentativeAccept(string type = null) : base(type ?? nameof(TentativeAccept)) { }
        }

        public class Travel : ActivityObject
        {
            public Travel(string type = null) : base(type ?? nameof(Travel)) { }
        }

        public class Undo : ActivityObject
        {
            public Undo(string type = null) : base(type ?? nameof(Undo)) { }
        }

        public class Update : ActivityObject
        {
            public Update(string type = null) : base(type ?? nameof(Update)) { }
        }

        public class View : ActivityObject
        {
            public View(string type = null) : base(type ?? nameof(View)) { }
        }
    }
}