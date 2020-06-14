using System.Threading.Tasks;
using PinkUmbrella.Services;

namespace PinkUmbrella.Services.Sql
{
    public class ArchiveService : IArchiveService
    {
        public ArchiveService()
        {
        }

        public Task<object> GetImage(int id)
        {
            throw new System.NotImplementedException();
        }

        public Task<object> GetVideo(int id)
        {
            throw new System.NotImplementedException();
        }
    }
}