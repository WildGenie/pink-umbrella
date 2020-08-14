using System.Threading.Tasks;
using PinkUmbrella.Repositories;
using PinkUmbrella.Models.Search;
using Estuary.Services;
using Estuary.Objects;

namespace PinkUmbrella.Services.Elastic.Search
{
    public class ElasticSearchArchivedPhotosService : ElasticSearchArchiveService
    {
        public ElasticSearchArchivedPhotosService(SimpleDbContext dbContext, IArchiveService archive, IHazActivityStreamPipe pipe): base(dbContext, archive, pipe)
        {
        }

        public override SearchResultType ResultType => SearchResultType.Photo;

        public override Task<SearchResultsModel> Search(SearchRequestModel request) => Search(request, CommonMediaType.Photo);
    }
}