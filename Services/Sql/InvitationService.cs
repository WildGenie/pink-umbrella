using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models;
using PinkUmbrella.Repositories;

namespace PinkUmbrella.Services.Sql
{
    public class InvitationService: IInvitationService
    {
        private readonly SignInManager<UserProfileModel> _signInManager;
        private readonly UserManager<UserProfileModel> _userManager;
        private readonly RoleManager<UserGroupModel> _roleManager;
        private readonly SimpleDbContext _dbContext;
        private readonly StringRepository _strings;

        public InvitationService(
                UserManager<UserProfileModel> userManager,
                SignInManager<UserProfileModel> signInManager,
                RoleManager<UserGroupModel> roleManager,
                SimpleDbContext dbContext,
                StringRepository strings
                ) {
                    _userManager = userManager;
                    _signInManager = signInManager;
                    _roleManager = roleManager;
                    _dbContext = dbContext;
                    _strings = strings;
                }

        public async Task ConsumeAccessCodeAsync(UserProfileModel user, GroupAccessCodeModel c)
        {
            if (c.WhenExpires >= DateTime.UtcNow)
            {
                if (c.Type == InvitationType.AddMeToGroup)
                {
                    await _userManager.AddToRoleAsync(user, c.GroupName);
                }
                c.WhenConsumed = DateTime.UtcNow;
                if (c.ForUserId == 0)
                {
                    c.ForUserId = user.Id;
                }
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<GroupAccessCodeModel> NewGroupAccessCode(GroupAccessCodeModel info)
        {
            info.Type = InvitationType.AddMeToGroup;
            info.Code = Guid.NewGuid().ToString();
            info.WhenCreated = DateTime.UtcNow;
            info.WhenExpires = DateTime.UtcNow.AddDays(1);
            await _dbContext.GroupAccessCodes.AddAsync(info);
            await _dbContext.SaveChangesAsync();
            return info;
        }

        public Task<GroupAccessCodeModel> NewGroupAccessCode(int createdByUserId, int forUserId, string group)
        {
            return NewGroupAccessCode(new GroupAccessCodeModel() {
                CreatedByUserId = createdByUserId,
                ForUserId = forUserId,
                GroupName = group,
            });
        }

        public async Task<List<GroupAccessCodeModel>> GetUnusedUnexpiredAccessCodes()
        {
            var now = DateTime.UtcNow;
            return await _dbContext.GroupAccessCodes.Where(c => c.WhenConsumed == null && c.WhenExpires < now).ToListAsync();
        }

        public async Task<GroupAccessCodeModel> GetAccessCodeAsync(string code, int? userId, InvitationType? type)
        {
            var c = await _dbContext.GroupAccessCodes.SingleOrDefaultAsync(c => c.WhenConsumed == null && c.Code == code);
            if (c != null && (!userId.HasValue || c.ForUserId == userId.Value) && (!type.HasValue || c.Type == type) && c.WhenExpires >= DateTime.UtcNow)
            {
                return c;
            }
            else
            {
                return null;
            }
        }

        public async Task<GroupAccessCodeModel> NewInvitation(int createdByUserId, InvitationType type, int forUserId, string message, int daysValidFor)
        {
            var info = new GroupAccessCodeModel()
            {
                Type = InvitationType.Register,
                CreatedByUserId = createdByUserId,
                ForUserId = forUserId,
                GroupName = message,
                Code = Guid.NewGuid().ToString(),
                WhenCreated = DateTime.UtcNow,
                WhenExpires = DateTime.UtcNow.AddDays(daysValidFor)
            };
            await _dbContext.GroupAccessCodes.AddAsync(info);
            await _dbContext.SaveChangesAsync();
            return info;
        }

        public async Task<List<GroupAccessCodeModel>> GetInvitesToUser(int userId)
        {
            var now = DateTime.UtcNow;
            return await _dbContext.GroupAccessCodes.Where(code => code.ForUserId == userId && code.WhenConsumed == null && code.WhenExpires > now).ToListAsync();
        }

        public async Task<List<GroupAccessCodeModel>> GetInvitesFromUser(int userId)
        {
            var now = DateTime.UtcNow;
            return await _dbContext.GroupAccessCodes.Where(code => code.CreatedByUserId == userId && code.WhenConsumed == null && code.WhenExpires > now).ToListAsync();
        }

        public async Task<GroupAccessCodeModel> GetAccessCodeAsync(int id)
        {
            return await _dbContext.GroupAccessCodes.FindAsync(id);
        }
    }
}