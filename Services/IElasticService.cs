using System.Collections.Generic;
using System.Threading.Tasks;
using PinkUmbrella.Models.Elastic;

namespace PinkUmbrella.Services
{
    public interface IElasticService
    {
        Task SyncProfiles(long authId, List<ElasticProfileModel> profiles);
    }
}