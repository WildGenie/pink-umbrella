using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Estuary.Activities;
using Estuary.Actors;
using Estuary.Core;
using Estuary.Objects;
using Tides.Models;
using Tides.Models.Public;
using static Estuary.Activities.Common;
using static Estuary.Actors.Common;
using static Estuary.Objects.Common;

namespace Estuary.Streams.Json
{
    public class CustomJsonSerializer
    {
        private readonly List<Func<Stream, Stream>> _streamInputWrappers = new List<Func<Stream, Stream>>();
        private readonly List<Func<Stream, Stream>> _streamOutputWrappers = new List<Func<Stream, Stream>>();
        private readonly List<Func<object, object>> _objectInputWrappers = new List<Func<object, object>>();
        private readonly List<Func<object, object>> _objectOutputWrappers = new List<Func<object, object>>();
        private readonly List<Func<IEnumerable<object>, object>> _arrayInputWrappers = new List<Func<IEnumerable<object>, object>>();
        private readonly List<Func<IEnumerable<object>, object>> _arrayOutputWrappers = new List<Func<IEnumerable<object>, object>>();
        private readonly List<Func<CustomProperty, object>> _propertyInputWrappers = new List<Func<CustomProperty, object>>();
        private readonly List<Func<IEnumerable<CustomProperty>, IEnumerable<CustomProperty>>> _propertyOutputWrappers = new List<Func<IEnumerable<CustomProperty>, IEnumerable<CustomProperty>>>();

        private readonly Dictionary<Type, Func<object, Stream, CustomJsonSerializer, Task>> _valueSerializers = new Dictionary<Type, Func<object, Stream, CustomJsonSerializer, Task>>();

        private readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions();

        public CustomJsonSerializer()
        {
            AddValueSerializer<bool>(SerializeBool);
            AddValueSerializer<long>(SerializeLong);
            AddValueSerializer<int>(SerializeInt);
            AddValueSerializer<double>(SerializeDouble);
            AddValueSerializer<float>(SerializeFloat);
            AddValueSerializer<char>(SerializeChar);
            AddValueSerializer<DateTime>(SerializeDateTime);
            AddValueSerializer<PublicId>(SerializePublicId);
            AddValueSerializer<CollectionObject>(SerializeCollection);

            AddPropertyOutputWrapper(ps => ps.Where(p => p.Value != null && (!(p.Value is BaseObject obj) || obj.IsDefined)));
            AddPropertyOutputWrapper(ps => ps.Where(p => !Attribute.IsDefined(p.Property, typeof(JsonIgnoreAttribute))));
            AddPropertyOutputWrapper(ps =>
            {
                var mapped = ps.ToDictionary(k => k.Name, v => v);
                var ret = new List<CustomProperty>();
                var missingRequiredFields = new List<string>();
                foreach (var p in new string[] {"type", "id"})
                {
                    if (mapped.Remove(p, out var t))
                    {
                        ret.Add(t);
                    }
                    else
                    {
                        missingRequiredFields.Add(p);
                    }
                }
                if (missingRequiredFields.Count > 1)
                {
                    throw new ArgumentNullException(string.Join(", ", missingRequiredFields));
                }
                // else if (missingRequiredFields.Contains("type"))
                // {
                //     throw new ArgumentNullException("type");
                // }
                ret.AddRange(mapped.Values.OrderBy(p => p.Name));
                return ret;
            });

            var coreConverter = new CoreObjectConverter();
            serializerOptions.Converters.Add(new ObjectCollectionConverter(coreConverter));
            serializerOptions.Converters.Add(new ObjectConverter(coreConverter));
            serializerOptions.Converters.Add(new JsonStringEnumConverter<Visibility>());
            serializerOptions.Converters.Add(new NullableDateTimeConverter());
            serializerOptions.Converters.Add(new DateTimeConverter());
        }

        public void AddStreamInputWrapper(Func<Stream, Stream> wrap) => _streamInputWrappers.Add(wrap);
        
        public void AddStreamOutputWrapper(Func<Stream, Stream> wrap) => _streamOutputWrappers.Add(wrap);

        public void AddObjectInputWrapper(Func<object, object> wrap) => _objectInputWrappers.Add(wrap);
        
        public void AddObjectOutputWrapper(Func<object, object> wrap) => _objectInputWrappers.Add(wrap);

        public void AddArrayInputWrapper(Func<IEnumerable<object>, object> wrap) => _arrayInputWrappers.Add(wrap);
        
        public void AddArrayOutputWrapper(Func<IEnumerable<object>, object> wrap) => _arrayOutputWrappers.Add(wrap);

        public void AddPropertyInputWrapper(Func<CustomProperty, object> wrap) => _propertyInputWrappers.Add(wrap);
        
        public void AddPropertyOutputWrapper(Func<IEnumerable<CustomProperty>, IEnumerable<CustomProperty>> wrapper) => _propertyOutputWrappers.Add(wrapper);
        
        public void AddValueSerializer<T>(Func<T, Stream, CustomJsonSerializer, Task> serializer) => _valueSerializers.Add(typeof(T), (object v, Stream dest, CustomJsonSerializer serializer2) => serializer((T)v, dest, serializer2));

        public StreamReader OpenReader(Stream source)
        {
            var wrapSrc = source;
            foreach (var streamWrapper in _streamInputWrappers)
            {
                wrapSrc = streamWrapper.Invoke(wrapSrc);
            }
            return new StreamReader(wrapSrc);
        }

        public async Task<BaseObject> Deserialize(StreamReader source) => await DeserializeObject<ActivityObject>(source);

        private static readonly Regex FindTypeRegex = new Regex(@"(?:""type""\s*:\s*"")(?<t>\w+)(?:"")");

        private async Task<T> DeserializeObject<T>(StreamReader src) where T: BaseObject
        {
            string json;
            //var reader = new StreamReader(src);
            //do
            //{
                json = await src.ReadLineAsync();
            //} while (!json.EndsWith('}'));
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            var peekType = json.IndexOf("\"type\"");
            if (peekType <= 0 || peekType >= json.Length)
            {
                throw new JsonException();
            }
            var peekStr = json.Substring(Math.Max(0, peekType - 4), Math.Min(json.Length - peekType, peekType + 30));
            var peek = FindTypeRegex.Match(peekStr).Groups["t"].Value;
            var type = TypeOf(peek);
            if (!typeof(T).IsAssignableFrom(type))
            {
                throw new JsonException($"Cannot assign {type.Name} to {typeof(T).Name}");
            }
            return (T) System.Text.Json.JsonSerializer.Deserialize(json, type, serializerOptions);
        }

        private void AssertByte(int actual, char expected, string errorMsg)
        {
            if (actual != expected)
            {
                throw new Exception(errorMsg);
            }
        }

        public async Task Serialize(BaseObject obj, Stream dest)
        {
            var wrapDest = dest;
            foreach (var streamWrapper in _streamOutputWrappers)
            {
                wrapDest = streamWrapper.Invoke(wrapDest);
            }

            await SerializeObject(obj, dest);
        }

        private async Task SerializeObject(BaseObject obj, Stream dest)
        {
            var props = GetProperties(obj);
            switch (props.Count)
            {
                case 0:
                await SerializeNull(dest);
                return;
                case 1:
                var first = props.First();
                if (first.Name == "id" || first.Name == "url" || first.Name == "href")
                {
                    SerializeString(first.Value?.ToString(), dest);
                    return;
                }
                break;
            }
            dest.WriteByte((byte)'{');
            await SerializeProperties(props, dest);
            dest.WriteByte((byte)'}');
        }

        private async Task SerializeProperties(IEnumerable<CustomProperty> props, Stream dest)
        {
            foreach (var propWrapper in _propertyOutputWrappers)
            {
                props = propWrapper(props);
            }
            var propList = props.ToList();
            if (propList.Count > 0)
            {
                await SerializeProperty(propList.First(), dest);
                foreach (var prop in propList.Skip(1))
                {
                    dest.WriteByte(44); // ,
                    await SerializeProperty(prop, dest);
                }
            }
        }

        private async Task SerializeProperty(CustomProperty prop, Stream dest)
        {
            SerializeString(prop.Name, dest);
            dest.WriteByte(58); // :
            await SerializeValue(prop.Value, dest);
        }

        private Task SerializeValue(object value, Stream dest)
        {
            if (value == null)
            {
                return SerializeNull(dest);
            }
            else if (value is string valueString)
            {
                SerializeString(valueString, dest);
                return Task.CompletedTask;
            }
            else if (value.GetType().IsEnum)
            {
                SerializeString(value.ToString(), dest);
                return Task.CompletedTask;
            }
            else if (value is CollectionObject collection)
            {
                return SerializeCollection(collection, dest, this);
            }
            else if (value is BaseObject obj)
            {
                return SerializeObject(obj, dest);
            }
            else if (value is List<BaseObject> array)
            {
                return SerializeArray(array, dest);
            }
            else if (_valueSerializers.TryGetValue(value.GetType(), out var directConversion))
            {
                return directConversion.Invoke(value, dest, this);
            }
            else
            {
                var indirectConversion = _valueSerializers.Where(t => value.GetType().IsInstanceOfType(t.Key)).Select(t => t.Value).FirstOrDefault();
                if (indirectConversion != null)
                {
                    return indirectConversion.Invoke(value, dest, this);
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        private async Task SerializeCollection(CollectionObject collection, Stream dest, CustomJsonSerializer serializer)
        {
            if (collection.IsBaseObjectDefined)
            {
                await SerializeObject(collection, dest);
            }
            else if (collection.items != null)
            {
                await SerializeArray(collection.items, dest);
            }
            else
            {
                await SerializeNull(dest);
            }
        }

        private async Task SerializeArray(List<BaseObject> items, Stream dest)
        {
            if (items.Count > 0)
            {
                dest.WriteByte((byte)'[');
                await SerializeValue(items.First(), dest);
                foreach (var obj in items.Skip(1))
                {
                    dest.WriteByte(44); // ,
                    await SerializeValue(obj, dest);
                }
                dest.WriteByte((byte)']');
            }
            else
            {
                await SerializeNull(dest);
            }
        }

        private async Task SerializeNull(Stream dest) => await dest.WriteAsync(System.Text.Encoding.UTF8.GetBytes("null"));

        private void SerializeString(string name, Stream dest)
        {
            dest.WriteByte(34); // "
            foreach (var ch in System.Text.Encoding.UTF8.GetBytes(name))
            {
                switch (ch)
                {
                    case 34: // "
                    dest.WriteByte(92);
                    dest.WriteByte(34);
                    break;
                    case 92: // \
                    dest.WriteByte(92);
                    dest.WriteByte(92);
                    break;
                    // case 47: // /
                    // dest.WriteByte(92);
                    // dest.WriteByte(47);
                    // break;
                    case 8: // <bs>
                    break;
                    case 12: // <feed>
                    break;
                    case 10: // \n
                    dest.WriteByte(92);
                    dest.WriteByte(110);
                    break;
                    case 13: // \r
                    dest.WriteByte(92);
                    dest.WriteByte(114);
                    break;
                    case 9: // \t
                    dest.WriteByte(92);
                    dest.WriteByte(116);
                    break;
                    default:
                    dest.WriteByte(ch);
                    break;
                }
            }
            dest.WriteByte(34); // "
        }

        //private List<CustomProperty> GetProperties(BaseObject obj) => obj.GetType().GetProperties().Select(p => TransformProperty(p, obj)).ToList();
        private List<CustomProperty> GetProperties(BaseObject obj)
        {
            var props = obj.GetType().GetProperties().ToList();
            return props.Select(p => TransformProperty(p, obj)).ToList();
        }

        private CustomProperty TransformProperty(PropertyInfo p, object obj)
        {
            string name;
            if (Attribute.IsDefined(p, typeof(JsonPropertyNameAttribute)))
            {
                name = ((JsonPropertyNameAttribute)p.GetCustomAttribute(typeof(JsonPropertyNameAttribute))).Name;
            }
            else
            {
                name = $"{Char.ToLower(p.Name.First())}{p.Name.Substring(1)}";
            }
            return new CustomProperty { Property = p, Value = p.GetValue(obj), Name = name };
        }

        private Task SerializeBool(bool arg, Stream dest, CustomJsonSerializer serializer)
        {
            dest.Write(System.Text.Encoding.UTF8.GetBytes(arg ? "true" : "false"));
            return Task.CompletedTask;
        }

        private Task SerializeLong(long arg, Stream dest, CustomJsonSerializer serializer)
        {
            dest.Write(System.Text.Encoding.UTF8.GetBytes(arg.ToString()));
            return Task.CompletedTask;
        }

        private Task SerializeInt(int arg, Stream dest, CustomJsonSerializer serializer)
        {
            dest.Write(System.Text.Encoding.UTF8.GetBytes(arg.ToString()));
            return Task.CompletedTask;
        }
        
        private Task SerializeFloat(float arg, Stream dest, CustomJsonSerializer serializer)
        {
            dest.Write(System.Text.Encoding.UTF8.GetBytes(arg.ToString()));
            return Task.CompletedTask;
        }
        
        private Task SerializeDouble(double arg, Stream dest, CustomJsonSerializer serializer)
        {
            dest.Write(System.Text.Encoding.UTF8.GetBytes(arg.ToString()));
            return Task.CompletedTask;
        }
        
        private Task SerializeChar(char arg, Stream dest, CustomJsonSerializer serializer)
        {
            SerializeString(arg.ToString(), dest);
            return Task.CompletedTask;
        }
        
        private Task SerializeDateTime(DateTime arg, Stream dest, CustomJsonSerializer serializer)
        {
            serializer.SerializeString(arg.ToString(), dest);
            return Task.CompletedTask;
        }
        
        private Task SerializePublicId(PublicId arg, Stream dest, CustomJsonSerializer serializer)
        {
            serializer.SerializeString(arg.ToString(), dest);
            return Task.CompletedTask;
        }

        private class CoreObjectConverter
        {
            public BaseObject Deserialize(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartObject:
                    break;
                    case JsonTokenType.String:
                    return new BaseObject { id = reader.GetString() };
                    default:
                    throw new JsonException();
                }

                BaseObject ret = null;
                if (!reader.Read() || reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                string propertyName = reader.GetString();
                if (propertyName != "type")
                {
                    throw new JsonException($"First property was not type (was {propertyName})");
                }
                var type = JsonSerializer.Deserialize<string>(ref reader, options);
                ret = (BaseObject) Activator.CreateInstance(TypeOf(type));

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        return ret;
                    }

                    // Get the key.
                    if (reader.TokenType != JsonTokenType.PropertyName)
                    {
                        throw new JsonException();
                    }

                    propertyName = reader.GetString();
                    propertyName = Char.ToUpper(propertyName.First()) + propertyName.Substring(1);
                    var prop = ret.GetType().GetProperty(propertyName);

                    if (prop == null)
                    {
                        prop = ret.GetType().GetProperties().FirstOrDefault(p =>
                        {
                            var isProp = string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase);
                            if (!isProp)
                            {
                                var jsonProp = p.GetCustomAttribute<JsonPropertyNameAttribute>();
                                isProp = jsonProp != null && string.Equals(jsonProp.Name, propertyName, StringComparison.OrdinalIgnoreCase);
                            }
                            return isProp;
                        });
                        if (prop == null)
                        {
                            throw new Exception($"No such property {propertyName} on {type}");
                        }
                    }

                    object propValue;
                    try
                    {
                        propValue = JsonSerializer.Deserialize(ref reader, prop.PropertyType, options);
                        if (prop.CanWrite)
                        {
                            prop.SetValue(ret, propValue);
                        }
                    }
                    catch (Exception e)
                    {
                        throw new JsonException($"Exception while binding {propertyName}", e);
                    }
                    propertyName = null;
                }
                throw new JsonException();
            }
        }

        private class ObjectConverter : JsonConverter<BaseObject>
        {
            private readonly CoreObjectConverter _coreConverter;

            public ObjectConverter(CoreObjectConverter coreConverter) : base()
            {
                _coreConverter = coreConverter;
            }

            public override BaseObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => _coreConverter.Deserialize(ref reader, typeToConvert, options);

            public override void Write(Utf8JsonWriter writer, BaseObject value, JsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }
        }

        private class ObjectCollectionConverter : JsonConverter<CollectionObject>
        {
            private readonly CoreObjectConverter _coreConverter;

            public ObjectCollectionConverter(CoreObjectConverter coreConverter) : base()
            {
                _coreConverter = coreConverter;
            }

            public override CollectionObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.StartArray)
                {
                    var ret = new CollectionObject();
                    ret.items = new List<BaseObject>();
                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonTokenType.EndArray)
                        {
                            return ret;
                        }

                        switch (reader.TokenType)
                        {
                            case JsonTokenType.EndArray: return ret;
                            case JsonTokenType.StartObject:
                            //ret.items.Add((BaseObject) JsonSerializer.Deserialize(ref reader, typeof(BaseObject), options));
                            ret.items.Add(_coreConverter.Deserialize(ref reader, typeof(BaseObject), options));
                            break;
                            case JsonTokenType.String: ret.items.Add(new BaseObject { id = reader.GetString() }); break;
                            case JsonTokenType.Null: ret.items.Add(null); break;
                            default: throw new JsonException();
                        }
                    }
                    throw new JsonException();
                }
                else
                {
                    var res = _coreConverter.Deserialize(ref reader, typeToConvert, options);
                    return res as CollectionObject;
                }
            }

            public override void Write(Utf8JsonWriter writer, CollectionObject value, JsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }
        }
        
        public static Type TypeOf(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                throw new ArgumentNullException(nameof(type));
            }

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
                case nameof(Downvote):               return typeof(Downvote);
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
                case nameof(Upvote):                return typeof(Upvote);
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
    }
}