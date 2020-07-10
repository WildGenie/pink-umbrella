using System.Threading.Tasks;
using PinkUmbrella.Models;
using PinkUmbrella.Repositories;
using PinkUmbrella.Models.Search;


namespace PinkUmbrella.Services.Sql.Search
{
    public class SearchArchivedPhotosService : SearchArchiveService
    {
        public SearchArchivedPhotosService(SimpleDbContext dbContext, IArchiveService archive): base(dbContext, archive)
        {
        }

        public override SearchResultType ResultType => SearchResultType.Photo;

        public override Task<SearchResultsModel> Search(string text, int? viewerId, SearchResultOrder order, PaginationModel pagination)
        {
            return Search(text, viewerId, ArchivedMediaType.Photo, order, pagination);
        }
    }
}