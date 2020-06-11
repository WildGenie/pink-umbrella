using System.Collections.Generic;
using seattle.Models;

namespace seattle.Services.Sql
{
    public class PostService : IPostService
    {
        public PostModel CreatePosts(List<PostModel> post_chain)
        {
            throw new System.NotImplementedException();
        }

        public void DeletePost(int id, int userId)
        {
            throw new System.NotImplementedException();
        }

        public PostModel GetPost(int id)
        {
            throw new System.NotImplementedException();
        }

        public void UpdateShadowBanStatus(int id, bool status)
        {
            throw new System.NotImplementedException();
        }

        public List<PostModel> UserPosts(int userId, int viewerId, PaginationModel pagination)
        {
            throw new System.NotImplementedException();
        }
    }
}