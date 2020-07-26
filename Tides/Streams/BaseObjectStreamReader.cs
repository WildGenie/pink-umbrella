using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tides.Activities;
using Tides.Core;
using Tides.Objects;
using static Tides.Activities.Common;
using static Tides.Actors.Common;
using static Tides.Objects.Common;

namespace Tides.Streams
{
    public abstract class BaseObjectStreamReader: BaseObjectStream
    {
        private static readonly Regex FindTypeRegex = new Regex(@"(?:{\s+""type""\s+:\s+"")(?<t>\w+)(?:"".*)");
        
        public abstract Task<BaseObject> Read();

        public Type TypeOf(string type)
        {
            switch (type)
            {
                case nameof(Accept):                return typeof(Accept);
                case nameof(Add):                   return typeof(Add);
                case nameof(Announce):              return typeof(Announce);
                case nameof(Arrive):                return typeof(Arrive);
                case nameof(Block):                 return typeof(Block);
                case nameof(Create):                return typeof(Create);
                case nameof(Delete):                return typeof(Delete);
                case nameof(Dislike):               return typeof(Dislike);
                case nameof(Flag):                  return typeof(Flag);
                case nameof(Follow):                return typeof(Follow);
                case nameof(Ignore):                return typeof(Ignore);
                case nameof(Invite):                return typeof(Invite);
                case nameof(Join):                  return typeof(Join);
                case nameof(Leave):                 return typeof(Leave);
                case nameof(Like):                  return typeof(Like);
                case nameof(Listen):                return typeof(Listen);
                case nameof(Move):                  return typeof(Move);
                case nameof(Offer):                 return typeof(Offer);
                case nameof(Question):              return typeof(Question);
                case nameof(Reject):                return typeof(Reject);
                case nameof(Read):                  return typeof(Read);
                case nameof(Remove):                return typeof(Remove);
                case nameof(TentativeAccept):       return typeof(TentativeAccept);
                case nameof(TentativeReject):       return typeof(TentativeReject);
                case nameof(Travel):                return typeof(Travel);
                case nameof(Undo):                  return typeof(Undo);
                case nameof(Update):                return typeof(Update);
                case nameof(View):                  return typeof(View);


                case nameof(Application):           return typeof(Application);
                case nameof(Actors.Common.Group):   return typeof(Actors.Common.Group);
                case nameof(Organization):          return typeof(Organization);
                case nameof(Person):                return typeof(Person);
                case nameof(Service):               return typeof(Service);


                case nameof(Article):               return typeof(Article);
                case nameof(Document):              return typeof(Document);
                case nameof(Audio):                 return typeof(Audio);
                case nameof(Event):                 return typeof(Event);
                case nameof(Image):                 return typeof(Image);
                case nameof(Mention):               return typeof(Mention);
                case nameof(Note):                  return typeof(Note);
                case nameof(Page):                  return typeof(Page);
                case nameof(Place):                 return typeof(Place);
                case nameof(Profile):               return typeof(Profile);
                case nameof(Relationship):          return typeof(Relationship);
                case nameof(Tombstone):             return typeof(Tombstone);
                case nameof(Video):                 return typeof(Video);

                default: throw new ArgumentException($"Invalid type {type}");
            }
        }

        protected Type ResolveType(byte[] peek)
        {
            return TypeOf(FindTypeRegex.Match(System.Text.Encoding.UTF8.GetString(peek)).Groups["t"].Value);
        }
    }
}