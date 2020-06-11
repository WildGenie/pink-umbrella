using System.Collections.Generic;
using seattle.Models;

namespace seattle.Services
{
    public interface IPostService
    {
        PostModel GetPost(int id);
        List<PostModel> UserPosts(int userId, int viewerId, PaginationModel pagination);

        PostModel CreatePosts(List<PostModel> post_chain);
        void DeletePost(int id, int userId);
        void UpdateShadowBanStatus(int id, bool status);
    }
}