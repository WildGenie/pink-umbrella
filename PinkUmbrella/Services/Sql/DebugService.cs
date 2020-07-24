using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models;
using PinkUmbrella.Repositories;
using Tides.Models;
using Tides.Models.Public;

namespace PinkUmbrella.Services.Sql
{
    public class DebugService : IDebugService
    {
        private readonly LogDbContext _dbContext;

        public DebugService(LogDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PaginatedModel<LoggedExceptionModel>> Get(PaginationModel pagination)
        {
            return new PaginatedModel<LoggedExceptionModel>() {
                Items = await _dbContext.Exceptions.OrderByDescending(e => e.Id).Skip(pagination.start).Take(pagination.count).ToListAsync(),
                Total = _dbContext.Exceptions.Count(),
                Pagination = pagination
            };
        }

        public async Task Log(Exception ex, string RequestCode, int? userId)
        {
            _dbContext.Exceptions.Add(new LoggedExceptionModel() {
                RequestCode = RequestCode,
                UserId = userId,
                Logged = DateTime.UtcNow,
                Message = ex.Message,
                Source = ex.Source,
                StackTrace = ex.StackTrace,
                Serialized = JsonSerializer.Serialize(ex.Data),
                Stringified = ex.ToString(),
            });
            await _dbContext.SaveChangesAsync();
        }
    }
}