using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models;
using PinkUmbrella.Models.AhPushIt;
using PinkUmbrella.Models.Public;
using PinkUmbrella.Repositories;
using Tides.Models;
using Tides.Models.Public;

namespace PinkUmbrella.Services.Sql
{
    public class NotificationService : INotificationService
    {
        private readonly AhPushItDbContext _db;

        public NotificationService(AhPushItDbContext db)
        {
            _db = db;
        }

        public async Task<List<NotificationMethodSetting>> GetMethodSettings(int userId)
        {
            var ret = await _db.MethodSettings.Where(s => s.UserId == userId).ToListAsync();
            var existing = ret.GroupBy(s => s.Type).ToDictionary(k => k.Key, v => v.Select(s => s.Method).ToHashSet());

            foreach (var x in Enum.GetValues(typeof(NotificationType)).Cast<NotificationType>())
            {
                if (existing.TryGetValue(x, out var existingMethods))
                {
                    foreach (var method in Enum.GetValues(typeof(NotificationMethod)).Cast<NotificationMethod>().Except(existingMethods))
                    {
                        ret.Add(new NotificationMethodSetting() {
                            Enabled = method == NotificationMethod.Default,
                            Method = method,
                            UserId = userId,
                            Type = x,
                        });
                    }
                }
                else
                {
                    foreach (var method in Enum.GetValues(typeof(NotificationMethod)).Cast<NotificationMethod>())
                    {
                        ret.Add(new NotificationMethodSetting() {
                            Enabled = method == NotificationMethod.Default,
                            Method = method,
                            UserId = userId,
                            Type = x,
                        });
                    }
                }
            }

            return ret;
        }

        public async Task<PaginatedModel<UserNotification>> GetNotifications(int userId, int? sinceId, bool includeViewed, bool includeDismissed, PaginationModel pagination)
        {
            var deliveredNewNotifications = false;
            var ret = _db.Recipients.Where(r => r.ToUserId == userId);
            if (!includeDismissed)
            {
                ret = ret.Where(n => n.WhenDismissed == null);
            }
            if (!includeViewed)
            {
                ret = ret.Where(n => n.WhenViewed == null);
            }
            
            IQueryable<Notification> notifs = _db.Notifications;
            if (sinceId.HasValue)
            {
                notifs = notifs.Where(n => n.Id > sinceId.Value);
            }
            var joined = ret.Join(notifs, r => r.NotificationId, n => n.Id, (r, n) => new { r, n });
            var notifsPaginated = await joined.OrderByDescending(n => n.n.Id).Skip(pagination.start).Take(pagination.count).ToListAsync();
            foreach (var notif in notifsPaginated)
            {
                if (notif.r.WhenDelivered == null)
                {
                    deliveredNewNotifications = true;
                    notif.r.WhenDelivered = DateTime.UtcNow;
                }
            }

            if (deliveredNewNotifications)
            {
                await _db.SaveChangesAsync();
            }
            return new PaginatedModel<UserNotification>()
            {
                Items = notifsPaginated.Select(e => new UserNotification(e.r, e.n)).ToList(),
                Pagination = pagination,
                Total = await joined.CountAsync(),
            };
        }

        public async Task Publish(Notification notification, PublicId[] recipients)
        {
            recipients = recipients.Where(id => id.Id > 0 && (id.PeerId != 0 || id.Id != notification.FromUserId)).ToArray();
            if (recipients.Length > 0)
            {
                notification.RecipientCount = recipients.Length;
                notification.WhenCreated = DateTime.UtcNow;
                await _db.Notifications.AddAsync(notification);
                await _db.SaveChangesAsync();

                var localRecipients = recipients.Where(id => id.PeerId == 0).Select(id => id.Id).ToArray();
                if (localRecipients.Length > 0)
                {
                    var whoToNotifyAndHow = await _db.MethodSettings.Where(ms => ms.Enabled && ms.Type == notification.Type && localRecipients.Contains(ms.UserId)).ToListAsync();
                    foreach (var defaultForRecipient in localRecipients.Except(whoToNotifyAndHow.Select(u => u.UserId)))
                    {
                        whoToNotifyAndHow.Add(new NotificationMethodSetting() {
                            Method = NotificationMethod.Default,
                            UserId = defaultForRecipient,
                        });
                    }

                    foreach (var notify in whoToNotifyAndHow)
                    {
                        await this.PublishViaMethod(notification, notify.Method, notify.UserId);
                    }
                }

                var externalRecipients = recipients.Where(id => id.PeerId == 0).Select(id => id.Id).ToArray();
                if (externalRecipients.Length > 0)
                {
                    throw new NotSupportedException();
                }
            }
        }

        private async Task PublishViaMethod(Notification notification, NotificationMethod method, int userId)
        {
            NotificationRecipient model = null;
            switch (method)
            {
                case NotificationMethod.Default:
                {
                    model = new NotificationRecipient() {
                        Method = method,
                        NotificationId = notification.Id,
                        ToUserId = userId,
                    };
                }
                break;
                case NotificationMethod.Email:
                {
                    model = new NotificationRecipient() {
                        Method = method,
                        NotificationId = notification.Id,
                        ToUserId = userId,
                        WhenDelivered = DateTime.UtcNow,
                    };

                    // send email;
                    notification.DeliveryCount++; // delivery successful
                }
                break;
                case NotificationMethod.SMS:
                {
                    model = new NotificationRecipient() {
                        Method = method,
                        NotificationId = notification.Id,
                        ToUserId = userId,
                        WhenDelivered = DateTime.UtcNow,
                        WhenViewed = DateTime.UtcNow,
                        WhenDismissed = DateTime.UtcNow,
                    };

                    // send text;
                    notification.DeliveryCount++; // delivery successful
                }
                break;
            }

            if (model != null)
            {
                await _db.Recipients.AddAsync(model);
                await _db.SaveChangesAsync();
            }
        }

        public async Task UpdateMethodSetting(NotificationMethodSetting setting)
        {
            if (setting.Id > 0)
            {
                var n = await _db.MethodSettings.FindAsync(setting.Id);
                if (n != null && n.Method == setting.Method && n.UserId == setting.UserId)
                {
                    n.Enabled = setting.Enabled;
                }
            }
            else
            {
                await _db.MethodSettings.AddAsync(setting);
            }
            await _db.SaveChangesAsync();
        }

        public async Task UpdateMethodSettings(int userId, Dictionary<NotificationType, List<NotificationMethod>> typeMethods)
        {
            var existing = await GetMethodSettings(userId);
            var existingTypeMethods = existing.GroupBy(g => g.Type).ToDictionary(k => k.Key, v => v.ToDictionary(k => k.Method, v => v));
            foreach (var type in existingTypeMethods)
            {
                if (typeMethods.TryGetValue(type.Key, out var methods))
                {
                    if (methods.Contains(NotificationMethod.None))
                    {
                        foreach (var method in type.Value)
                        {
                            method.Value.Enabled = method.Key == NotificationMethod.None;
                            if (method.Value.Id == 0)
                            {
                                await _db.MethodSettings.AddAsync(method.Value);
                            }
                        }
                    }
                    else
                    {
                        foreach (var method in type.Value)
                        {
                            method.Value.Enabled = method.Key != NotificationMethod.None && methods.Contains(method.Key);
                            if (method.Value.Id == 0)
                            {
                                await _db.MethodSettings.AddAsync(method.Value);
                            }
                        }
                    }
                }
            }
            await _db.SaveChangesAsync();
        }

        public async Task UpdateMethodSettingsSetAll(int userId, NotificationMethod method)
        {
            var existing = await _db.MethodSettings.Where(s => s.UserId == userId).ToListAsync();
            foreach (var s in existing)
            {
                s.Enabled = s.Method == method;
            }
            await _db.SaveChangesAsync();
        }

        public async Task<int> GetUnviewedNotificationsCount(int userId)
        {
            return await _db.Recipients.Where(r => r.ToUserId == userId && r.WhenDismissed == null && r.WhenViewed == null).CountAsync();
        }

        public async Task Dismiss(int userId, int notificationRecipientId)
        {
            var notif = await _db.Recipients.FindAsync(notificationRecipientId);
            if (notif.ToUserId == userId)
            {
                notif.WhenDismissed = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
        }

        public async Task Viewed(int userId, int notificationRecipientId)
        {
            var notif = await _db.Recipients.FindAsync(notificationRecipientId);
            if (notif.ToUserId == userId)
            {
                notif.WhenViewed = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
        }

        public async Task DismissSince(int userId, int notificationRecipientId)
        {
            var notifs = await _db.Recipients.Where(r => r.ToUserId == userId && r.Id < notificationRecipientId && r.WhenDismissed == null).ToListAsync();
            if (notifs.Count > 0)
            {
                foreach (var notif in notifs)
                {
                    notif.WhenDismissed = DateTime.UtcNow;
                }
                await _db.SaveChangesAsync();
            }
        }

        public async Task ViewedSince(int userId, int notificationRecipientId)
        {
            var notifs = await _db.Recipients.Where(r => r.ToUserId == userId && r.Id <= notificationRecipientId && r.WhenViewed == null).ToListAsync();
            if (notifs.Count > 0)
            {
                var recordViewCount = new Dictionary<int, Notification>();
                foreach (var notif in notifs)
                {
                    notif.WhenViewed = DateTime.UtcNow;
                    if (recordViewCount.TryGetValue(notif.NotificationId, out var n))
                    {
                        n.ViewCount++;
                    }
                    else
                    {
                        var tmp = await _db.Notifications.FindAsync(notif.NotificationId);
                        tmp.ViewCount++;
                        recordViewCount.Add(tmp.Id, tmp);
                    }
                }
                await _db.SaveChangesAsync();
            }
        }
    }
}