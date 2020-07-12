using System.Threading.Tasks;
using PinkUmbrella.Models;
using PinkUmbrella.Models.Search;
using PinkUmbrella.Repositories;

namespace PinkUmbrella.Services.Elastic.Search
{
    public class ElasticSearchArchivedVideosService : ElasticSearchArchiveService
    {
        public ElasticSearchArchivedVideosService(SimpleDbContext dbContext, IArchiveService archive): base(dbContext, archive)
        {
        }

        public override SearchResultType ResultType => SearchResultType.Video;

        public override Task<SearchResultsModel> Search(SearchRequestModel request) => Search(request, ArchivedMediaType.Video);
    }
}