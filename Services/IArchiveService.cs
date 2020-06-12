using System.Threading.Tasks;

namespace seattle.Services
{
    public interface IArchiveService
    {
        Task<object> GetVideo(int id);
        Task<object> GetImage(int id);
    }
}