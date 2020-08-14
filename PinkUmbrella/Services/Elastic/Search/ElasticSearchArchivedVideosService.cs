using System.Threading.Tasks;
using Estuary.Objects;
using Estuary.Services;
using PinkUmbrella.Models.Search;
using PinkUmbrella.Repositories;

namespace PinkUmbrella.Services.Elastic.Search
{
    public class ElasticSearchArchivedVideosService : ElasticSearchArchiveService
    {
        public ElasticSearchArchivedVideosService(SimpleDbContext dbContext, IArchiveService archive, IHazActivityStreamPipe pipe): base(dbContext, archive, pipe)
        {
        }

        public override SearchResultType ResultType => SearchResultType.Video;

        public override Task<SearchResultsModel> Search(SearchRequestModel request) => Search(request, CommonMediaType.Video);
    }
}