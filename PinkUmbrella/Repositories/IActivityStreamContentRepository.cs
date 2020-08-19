using System.Threading.Tasks;
using Estuary.Core;
using PinkUmbrella.Models;
using Tides.Models.Public;

namespace PinkUmbrella.Repositories
{
    public interface IActivityStreamContentRepository
    {
        Task<ObjectContentModel> GetContentModel(PublicId id);

        Task<BaseObject> Get(PublicId id, PublicId viewerId);

        Task BindSqlContent(BaseObject bindTo);
    }
}