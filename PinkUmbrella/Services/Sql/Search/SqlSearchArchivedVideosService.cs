using System.Threading.Tasks;
using PinkUmbrella.Models;
using PinkUmbrella.Models.Search;
using PinkUmbrella.Repositories;

namespace PinkUmbrella.Services.Sql.Search
{
    public class SqlSearchArchivedVideosService : SqlSearchArchiveService
    {
        public SqlSearchArchivedVideosService(SimpleDbContext dbContext, IArchiveService archive): base(dbContext, archive)
        {
        }

        public override SearchResultType ResultType => SearchResultType.Video;

        public override Task<SearchResultsModel> Search(SearchRequestModel request) => Search(request, ArchivedMediaType.Video);
    }
}