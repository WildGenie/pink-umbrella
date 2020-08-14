using System.Collections.Generic;
using System.Threading.Tasks;
using PinkUmbrella.Models.AhPushIt;
using Tides.Models;
using Tides.Models.Public;

namespace PinkUmbrella.Services
{
    public interface INotificationService
    {
        Task Publish(Notification notification, PublicId[] recipients);
        
        Task<int> GetUnviewedNotificationsCount(int userId);

        Task<PaginatedModel<UserNotification>> GetNotifications(int userId, int? sinceId, bool includeViewed, PaginationModel pagination);
        
        Task<List<NotificationMethodSetting>> GetMethodSettings(int userId);

        Task UpdateMethodSetting(NotificationMethodSetting setting);

        Task UpdateMethodSettings(int userId, Dictionary<NotificationType, List<NotificationMethod>> typeMethods);

        Task UpdateMethodSettingsSetAll(int userId, NotificationMethod method);

        Task Dismiss(int userId, int notificationRecipientId);
        
        Task Viewed(int userId, int notificationRecipientId);

        Task DismissSince(int userId, int notificationRecipientId);
        
        Task ViewedSince(int userId, int notificationRecipientId);
    }
}