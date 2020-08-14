using Estuary.Core;
using Tides.Util;

namespace Estuary.Activities
{
    public static class Common
    {
        [IsDocumented]
        public class Accept : ActivityObject
        {
            public Accept(string type, string baseType) : base(type ?? nameof(Accept), baseType ?? "Activity") { }

            public Accept(): this(null, null) { }
        }

        [IsDocumented]
        public class Add : ActivityObject
        {
            public Add(string type) : base(type ?? nameof(Add), "Activity") { }

            public Add(): this(null) { }
        }

        [IsDocumented]
        public class Announce : ActivityObject
        {
            public Announce(string type) : base(type ?? nameof(Announce), "Activity") { }

            public Announce(): this(null) { }
        }

        [IsDocumented]
        public class Arrive : ActivityObject
        {
            public Arrive(string type) : base(type ?? nameof(Arrive), "Activity") { }

            public Arrive(): this(null) { }
        }

        [IsDocumented]
        public class Block : Ignore
        {
            public Block(string type) : base(type ?? nameof(Block), nameof(Ignore)) { }

            public Block(): this(null) { }
        }

        [IsDocumented]
        public class Report : Ignore
        {
            public Report(string type) : base(type ?? nameof(Report), nameof(Ignore)) { }

            public Report(): this(null) { }
        }

        [IsDocumented]
        public class Create : ActivityObject
        {
            public Create(string type) : base(type ?? nameof(Create), "Activity") { }

            public Create(): this(null) { }
        }

        [IsDocumented]
        public class Delete : ActivityObject
        {
            public Delete(string type) : base(type ?? nameof(Delete), "Activity") { }

            public Delete(): this(null) { }
        }

        [IsDocumented]
        public class Dislike : ActivityObject
        {
            public Dislike(string type) : base(type ?? nameof(Dislike), "Activity") { }

            public Dislike(): this(null) { }
        }

        [IsDocumented]
        public class Downvote : ActivityObject
        {
            public Downvote(string type) : base(type ?? nameof(Downvote), "Activity") { }

            public Downvote(): this(null) { }
        }

        [IsDocumented]
        public class Flag : ActivityObject
        {
            public Flag(string type) : base(type ?? nameof(Flag), "Activity") { }

            public Flag(): this(null) { }
        }

        [IsDocumented]
        public class Follow : ActivityObject
        {
            public Follow(string type) : base(type ?? nameof(Follow), "Activity") { }

            public Follow(): this(null) { }
        }

        [IsDocumented]
        public class Ignore : ActivityObject
        {
            public Ignore(string type, string baseType) : base(type ?? nameof(Ignore), baseType ?? "Activity") { }

            public Ignore(): this(null, null) { }
        }

        [IsDocumented]
        public class Invite : Offer
        {
            public Invite(string type) : base(type ?? nameof(Invite), nameof(Offer)) { }

            public Invite(): this(null) { }
        }

        [IsDocumented]
        public class Join : ActivityObject
        {
            public Join(string type) : base(type ?? nameof(Join), "Activity") { }

            public Join(): this(null) { }
        }

        [IsDocumented]
        public class Leave : ActivityObject
        {
            public Leave(string type) : base(type ?? nameof(Leave), "Activity") { }

            public Leave(): this(null) { }
        }

        [IsDocumented]
        public class Like : ActivityObject
        {
            public Like(string type) : base(type ?? nameof(Like), "Activity") { }

            public Like(): this(null) { }
        }

        [IsDocumented]
        public class Listen : ActivityObject
        {
            public Listen(string type) : base(type ?? nameof(Listen), "Activity") { }

            public Listen(): this(null) { }
        }

        [IsDocumented]
        public class Move : ActivityObject
        {
            public Move(string type) : base(type ?? nameof(Move), "Activity") { }

            public Move(): this(null) { }
        }

        [IsDocumented]
        public class Offer : ActivityObject
        {
            public Offer(string type, string baseType) : base(type ?? nameof(Offer), baseType ?? "Activity") { }

            public Offer(): this(null, null) { }
        }

        [IsDocumented]
        public class Reject : ActivityObject
        {
            public Reject(string type, string baseType) : base(type ?? nameof(Reject), baseType ?? "Activity") { }

            public Reject(): this(null, null) { }
        }

        [IsDocumented]
        public class Read : ActivityObject
        {
            public Read(string type) : base(type ?? nameof(Read), "Activity") { }

            public Read(): this(null) { }
        }

        [IsDocumented]
        public class Remove : ActivityObject
        {
            public Remove(string type) : base(type ?? nameof(Remove), "Activity") { }

            public Remove(): this(null) { }
        }

        [IsDocumented]
        public class TentativeReject : Reject
        {
            public TentativeReject(string type) : base(type ?? nameof(TentativeReject), "Reject") { }

            public TentativeReject(): this(null) { }
        }

        [IsDocumented]
        public class TentativeAccept : Accept
        {
            public TentativeAccept(string type) : base(type ?? nameof(TentativeAccept), "Accept") { }

            public TentativeAccept(): this(null) { }
        }

        [IsDocumented]
        public class Travel : ActivityObject
        {
            public Travel(string type) : base(type ?? nameof(Travel), "Activity") { }

            public Travel(): this(null) { }
        }

        [IsDocumented]
        public class Undo : ActivityObject
        {
            public Undo(string type) : base(type ?? nameof(Undo), "Activity") { }

            public Undo(): this(null) { }
        }

        [IsDocumented]
        public class Update : ActivityObject
        {
            public Update(string type) : base(type ?? nameof(Update), "Activity") { }

            public Update(): this(null) { }
        }

        [IsDocumented]
        public class Upvote : ActivityObject
        {
            public Upvote(string type) : base(type ?? nameof(Upvote), "Activity") { }

            public Upvote(): this(null) { }
        }

        [IsDocumented]
        public class View : ActivityObject
        {
            public View(string type) : base(type ?? nameof(View), "Activity") { }

            public View(): this(null) { }
        }
    }
}