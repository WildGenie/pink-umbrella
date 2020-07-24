using System;
using System.Threading.Tasks;
using PinkUmbrella.Models;
using Tides.Models;

namespace PinkUmbrella.Services
{
    public interface IDebugService
    {
        Task<PaginatedModel<LoggedExceptionModel>> Get(PaginationModel pagination);

        Task Log(Exception ex, string RequestCode, int? userId);
    }
}