using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Estuary.Util;
using StackExchange.Redis;
using Tides.Models.Auth;
using Tides.Models.Public;

namespace PinkUmbrella.Repositories
{
    public class RedisRepository
    {
        private readonly ConnectionMultiplexer _conn;

        public RedisRepository(ConnectionMultiplexer conn)
        {
            _conn = conn;
        }

        public async Task<T> Get<T>(object id, string baseId = null) where T: IHazComputedId
        {
            var db = _conn.GetDatabase();
            var t = typeof(T);
            var ret = (T) Activator.CreateInstance(t, false);
            var fields = FieldsFor(t);
            var anyFieldsExisted = false;
            foreach (var f in fields)
            {
                var key = RedisId(baseId, id, t, f);
                var keyExists = await db.KeyExistsAsync(key);
                if (keyExists)
                {
                    anyFieldsExisted = true;
                    var val = await db.StringGetAsync(key);
                    var bytes = val.Box() as byte[];
                    f.SetValue(ret, int.Parse(System.Text.Encoding.ASCII.GetString(bytes))); // bytes.Length == 1 ? (int) bytes[0] : BitConverter.ToInt32(bytes)
                }
                else if (f.GetCustomAttribute<RedisValueAttribute>().IsRequired)
                {
                    throw new ArgumentNullException($"Missing redis key {key} for {t.Name}'s field {f.Name}");
                }
            }
            return anyFieldsExisted ? ret : default;
        }

        public async Task<string> FieldGet<T>(object id, string property, string baseId = null)
        {
            var db = _conn.GetDatabase();
            var t = typeof(T);
            var f = t.GetProperty(property) ?? throw new ArgumentOutOfRangeException(nameof(property), property, $"Property is not in type {t.FullName}");
            var key = RedisId(baseId, id, t, f);
            var keyExists = await db.KeyExistsAsync(key);
            if (keyExists)
            {
                return await db.StringGetAsync(key);
            }
            else if (f.GetCustomAttribute<RedisValueAttribute>()?.IsRequired ?? false)
            {
                throw new ArgumentNullException($"Missing redis key {key} for {t.Name}'s field {f.Name}");
            }
            else
            {
                return null;
            }
        }

        public async Task Set<T>(T value, string baseId = null) where T: IHazComputedId
        {
            var db = _conn.GetDatabase();
            var id = value.ComputedId;
            var t = typeof(T);
            var fields = FieldsFor(t);
            foreach (var f in fields)
            {
                var key = RedisId(baseId, id, t, f);
                await db.StringSetAsync(key, RedisValue.Unbox(f.GetValue(value)), flags: CommandFlags.FireAndForget);
            }
        }

        public async Task Delete<T>(T value, string baseId = null) where T: IHazComputedId
        {
            var db = _conn.GetDatabase();
            var id = value.ComputedId;
            var t = typeof(T);
            var fields = FieldsFor(t);
            foreach (var f in fields)
            {
                var key = RedisId(baseId, id, t, f);
                await db.KeyDeleteAsync(key, CommandFlags.FireAndForget);
            }
        }

        public async Task FieldSet<T>(object id, string property, string value, string baseId = null)
        {
            var db = _conn.GetDatabase();
            var t = typeof(T);
            var f = t.GetProperty(property);
            var key = RedisId(baseId, id, t, f);
            await db.KeyDeleteAsync(key, CommandFlags.FireAndForget);
        }

        public async Task FieldDelete<T>(string property, object id, string baseId)
        {
            var db = _conn.GetDatabase();
            var t = typeof(T);
            var f = t.GetProperty(property);
            var key = RedisId(baseId, id, t, f);
            await db.KeyDeleteAsync(key, CommandFlags.FireAndForget);
        }

        public async Task Increment(string id) => await _conn.GetDatabase().StringIncrementAsync(id, flags: CommandFlags.FireAndForget);

        public async Task Increment<T>(string property, object id, string baseId)
        {
            var t = typeof(T);
            var f = t.GetProperty(property);
            if (f == null)
            {
                throw new ArgumentException($"{property} is not a public property");
            }
            await Increment(RedisId(baseId, id, t, f));
        }

        private string RedisId(string baseId, object id, Type t, PropertyInfo f) => $"{baseId}{t.Name}-{f.Name}-{id}";

        private IEnumerable<PropertyInfo> FieldsFor(Type t) => t.GetProperties().Where(p => Attribute.IsDefined(p, typeof(RedisValueAttribute)));
    }
}