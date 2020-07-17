using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models;
using PinkUmbrella.Repositories;
using PinkUmbrella.Services.Search;
using PinkUmbrella.Models.Search;
using System.Collections.Generic;

namespace PinkUmbrella.Services.Sql.Search
{
    public abstract class SqlSearchArchiveService : ISearchableService
    {
        private readonly SimpleDbContext _dbContext;
        private readonly IArchiveService _archive;

        public SqlSearchArchiveService(SimpleDbContext dbContext, IArchiveService archive)
        {
            _dbContext = dbContext;
            _archive = archive;
        }

        public string ControllerName => "Archive";

        public abstract SearchResultType ResultType { get; }

        public SearchSource Source => SearchSource.Sql;

        public async Task<SearchResultsModel> Search(SearchRequestModel request, ArchivedMediaType mediaType)
        {
            var query = _dbContext.ArchivedMedia.Where(m => m.MediaType == mediaType);
            if (!string.IsNullOrWhiteSpace(request.text))
            {
                var textToLower = request.text.ToLower();
                query = query.Where(p => p.DisplayName.Contains(textToLower) || p.Description.Contains(textToLower) || p.Attribution.Contains(textToLower));
            }

            if (request.tags != null && request.tags.Length > 0)
            {
                var tags = await _dbContext.AllTags.Where(t => request.tags.Contains(t.Tag)).Select(t => t.Id).ToArrayAsync();
                query = query.Where(p => _dbContext.ArchivedMediaTags.FirstOrDefault(t => t.ToId == p.Id && tags.Contains(t.TagId)) != null);
            }
            
            switch (request.order) {
                case SearchResultOrder.Top:
                case SearchResultOrder.Hot:
                query = query.OrderBy(q => q.LikeCount);
                break;
                default:
                case SearchResultOrder.Latest:
                query = query.OrderBy(q => q.WhenCreated);
                break;
            }

            var searchResults = await query.ToListAsync();
            var results = new List<ArchivedMediaModel>();
            foreach (var r in searchResults)
            {
                await _archive.BindReferences(r, request.viewerId);
                if (_archive.CanView(r, request.viewerId))
                {
                    results.Add(r);
                }
            }
            return new SearchResultsModel() {
                Results = results.Skip(request.pagination.start).Take(request.pagination.count).Select(p => new SearchResultModel() {
                    Type = ResultType,
                    Value = p,
                }).ToList(),
                TotalResults = results.Count()
            };
        }

        public abstract Task<SearchResultsModel> Search(SearchRequestModel request);
    }
}