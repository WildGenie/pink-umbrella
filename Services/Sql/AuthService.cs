using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models.Auth;
using PinkUmbrella.Models.Auth.Permissions;
using PinkUmbrella.Repositories;

namespace PinkUmbrella.Services.Sql
{
    public class AuthService : IAuthService
    {
        private readonly AuthDbContext _db;
        private readonly Dictionary<AuthType, IAuthTypeHandler> _typeHandlers;

        private IQueryable<IPAddressModel> GetIPv4() => _db.IPs.Where(ip => ip.Type == IPType.IPv4);

        private IQueryable<IPAddressModel> GetIPv6() => _db.IPs.Where(ip => ip.Type == IPType.IPv6);

        public AuthService(AuthDbContext dbContext, IEnumerable<IAuthTypeHandler> typeHandlers)
        {
            _db = dbContext;
            _typeHandlers = typeHandlers.ToDictionary(k => k.Type, v => v);
        }

        public async Task ForgetIPs()
        {
            _db.IPs.RemoveRange(_db.IPs);
            await _db.SaveChangesAsync();
        }

        public async Task<AuthKey> GetOrAdd(AuthKey key)
        {
            var original = key.Id > 0 ? await _db.AuthKeys.FindAsync(key.Id) : !string.IsNullOrWhiteSpace(key.PublicKey) ? await _db.AuthKeys.FirstOrDefaultAsync(k => k.Type == key.Type && k.Format == key.Format && k.PublicKey == key.PublicKey) : null;
            if (original == null)
            {
                key.WhenAdded = DateTime.UtcNow;
                await _db.AuthKeys.AddAsync(key);
                await _db.SaveChangesAsync();
                original = key;
            }
            return original;
        }

        public async Task<IPAddressModel> GetOrRememberIP(IPAddress address)
        {
            var ipaddress = address.ToString();
            var ips = address.AddressFamily == AddressFamily.InterNetworkV6 ? GetIPv6() : GetIPv4();
            var ip = await ips.FirstOrDefaultAsync(ip => ip.Address == ipaddress);
            if (ip == null)
            {
                ip = new IPAddressModel()
                {
                    Type = address.AddressFamily == AddressFamily.InterNetworkV6 ? IPType.IPv6 : IPType.IPv4,
                    Address = ipaddress,
                };
                await _db.IPs.AddAsync(ip);
                await _db.SaveChangesAsync();
            }
            return ip;
        }

        public async Task<bool> IsTrusted(IPAddress address, AuthKey key)
        {
            var ipaddress = address.ToString();
            var ips = address.AddressFamily == AddressFamily.InterNetworkV6 ? GetIPv6() : GetIPv4();
            var ip = await ips.FirstOrDefaultAsync(ip => ip.Address == ipaddress);
            var inDb = await _db.AuthKeys.FirstOrDefaultAsync(k => k.Format == key.Format && k.Type == key.Type && k.PublicKey == key.PublicKey);
            return inDb != null && ip != null && ip.AuthId != null && inDb.Id == ip.Id;
        }

        public async Task Trust(IPAddress address, AuthKey key)
        {
            var ip = await GetOrRememberIP(address);
            var auth = await GetOrAdd(key);
            await _db.SaveChangesAsync();
            ip.AuthId = auth.Id;
            await _db.SaveChangesAsync();
        }

        public async Task Untrust(AuthKey key)
        {
            var ipQuery = key.Id >= 0 ? _db.IPs.Where(ip => ip.AuthId == key.Id) : _db.IPs.Where(ip => ip.PublicKey == key.PublicKey);
            var ips = await ipQuery.ToListAsync();
            if (ips.Count > 0)
            {
                foreach (var ip in ips)
                {
                    ip.AuthId = null;
                }
                await _db.SaveChangesAsync();
            }
        }

        bool SitePermissionDefault(SitePermission perm)
        {
            switch (perm)
            {
                case SitePermission.POST_PUT: return false;
            }
            return true;
        }

        public async Task<AuthKey> GetCurrent()
        {
            if (_db.AuthKeys.Count() == 0)
            {
                await _db.AuthKeys.AddAsync(await _typeHandlers[AuthType.OpenPGP].GenerateKey(HandshakeMethod.Default));
                await _db.SaveChangesAsync();
            }
            
            return await _db.AuthKeys.FirstAsync();
        }
    }
}