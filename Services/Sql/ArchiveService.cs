using System.Threading.Tasks;
using seattle.Services;

namespace seattle.Services.Sql
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