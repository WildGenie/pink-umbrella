using System.Threading.Tasks;

namespace PinkUmbrella.Services
{
    public interface IArchiveService
    {
        Task<object> GetVideo(int id);
        Task<object> GetImage(int id);
    }
}