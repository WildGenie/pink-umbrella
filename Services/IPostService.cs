using System.Collections.Generic;
using System.Threading.Tasks;
using seattle.Models;

namespace seattle.Services
{
    public interface IPostService
    {
        Task<PostModel> GetPost(int id);
        Task<NewPostResult> TryCreatePosts(List<PostModel> post_chain);
        Task<NewPostResult> TryCreateTextPosts(int userId, string[] post_chain, Visibility visibility);
        Task DeletePost(int id, int userId);
        Task UpdateShadowBanStatus(int id, bool status);
    }
}