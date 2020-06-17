using System.Threading.Tasks;
using PinkUmbrella.Models;
using PinkUmbrella.Repositories;

namespace PinkUmbrella.Services.Sql.Search
{
    public class SearchArchivedVideosService : SearchArchiveService
    {
        public SearchArchivedVideosService(SimpleDbContext dbContext, IArchiveService archive): base(dbContext, archive)
        {
        }

        public override SearchResultType ResultType => SearchResultType.Video;

        public override Task<SearchResultsModel> Search(string text, int? viewerId, SearchResultOrder order, PaginationModel pagination)
        {
            return Search(text, viewerId, ArchivedMediaType.Video, order, pagination);
        }
    }
}