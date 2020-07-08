using System.Collections.Generic;
using System.Threading.Tasks;
using PinkUmbrella.Models;

namespace PinkUmbrella.Services
{
    public interface IInvitationService
    {
        Task<GroupAccessCodeModel> NewGroupAccessCode(int createdByUserId, int forUserId, string group);

        Task<GroupAccessCodeModel> GetAccessCodeAsync(string code, int? userId, InvitationType? type);

        Task<GroupAccessCodeModel> GetAccessCodeAsync(int id);
        
        Task ConsumeAccessCodeAsync(UserProfileModel user, GroupAccessCodeModel code);

        Task<List<GroupAccessCodeModel>> GetUnusedUnexpiredAccessCodes();

        Task<GroupAccessCodeModel> NewInvitation(int createdByUserId, InvitationType type, int forUserId, string message, int daysValidFor);
        
        Task<List<GroupAccessCodeModel>> GetInvitesToUser(int userId);

        Task<List<GroupAccessCodeModel>> GetInvitesFromUser(int userId);
    }
}