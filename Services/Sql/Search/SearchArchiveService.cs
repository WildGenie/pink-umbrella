using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models;
using PinkUmbrella.Repositories;
using System.Collections.Generic;

namespace PinkUmbrella.Services.Sql.Search
{
    public abstract class SearchArchiveService : ISearchableService
    {
        private readonly SimpleDbContext _dbContext;
        private readonly IArchiveService _archive;

        public SearchArchiveService(SimpleDbContext dbContext, IArchiveService archive)
        {
            _dbContext = dbContext;
            _archive = archive;
        }

        public string ControllerName => "Archive";

        public abstract SearchResultType ResultType { get; }

        public async Task<SearchResultsModel> Search(string text, int? viewerId, ArchivedMediaType type, SearchResultOrder order, PaginationModel pagination)
        {
            var query = _dbContext.ArchivedMedia.Where(m => m.MediaType == type);
            if (!string.IsNullOrWhiteSpace(text))
            {
                var textToLower = text.ToLower();
                query = query.Where(p => p.DisplayName.Contains(textToLower) || p.Description.Contains(textToLower));
            }
            
            switch (order) {
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
                await _archive.BindReferences(r, viewerId);
                if (_archive.CanView(r, viewerId))
                {
                    results.Add(r);
                }
            }
            return new SearchResultsModel() {
                Results = results.Skip(pagination.start).Take(pagination.count).Select(p => new SearchResultModel() {
                    Type = ResultType,
                    Value = p,
                }).ToList(),
                TotalResults = results.Count()
            };
        }

        public abstract Task<SearchResultsModel> Search(string text, int? viewerId, SearchResultOrder order, PaginationModel pagination);
    }
}