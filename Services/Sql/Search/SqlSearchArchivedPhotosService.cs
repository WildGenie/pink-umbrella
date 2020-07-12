using System.Threading.Tasks;
using PinkUmbrella.Models;
using PinkUmbrella.Repositories;
using PinkUmbrella.Models.Search;


namespace PinkUmbrella.Services.Sql.Search
{
    public class SqlSearchArchivedPhotosService : SqlSearchArchiveService
    {
        public SqlSearchArchivedPhotosService(SimpleDbContext dbContext, IArchiveService archive): base(dbContext, archive)
        {
        }

        public override SearchResultType ResultType => SearchResultType.Photo;

        public override Task<SearchResultsModel> Search(SearchRequestModel request) => Search(request, ArchivedMediaType.Photo);
    }
}