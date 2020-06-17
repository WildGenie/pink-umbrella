using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PinkUmbrella.Models;

namespace PinkUmbrella.Services
{
    public interface IArchiveService
    {
        Task<NewMediaResult> TryUploadMedias(List<ArchivedMediaModel> medias);

        Task<ArchivedMediaModel> GetMedia(int id, int? viewerId);

        Task<ArchivedMediaModel> GetMedia(string path, int? viewerId);
        
        Task<PaginatedModel<ArchivedMediaModel>> GetMediaForUser(int userId, int? viewerId, ArchivedMediaType? type, PaginationModel pagination);

        Task BindReferences(ArchivedMediaModel media, int? viewerId);

        bool CanView(ArchivedMediaModel media, int? viewerId);
        
        Task<ArchivedMediaModel> DeleteMedia(int id, int? viewerId);
        
        Task<MediaScanResultModel> ScanMediaForProfanity(ArchivedMediaModel media, int? viewerId);
        
        Task<MediaScanResultModel> ScanMediaForVirusOrBadThings(ArchivedMediaModel media, int? viewerId);
        
        Task<MediaScanResultModel> ScanMediaForKKKops(ArchivedMediaModel media, int? viewerId);

        Task<Stream> GetStream(ArchivedMediaModel media, int? viewerId);

        Task UpdateShadowBanStatus(int id, bool status);

        Task<PaginatedModel<ArchivedMediaModel>> GetMostReportedMedia();
        
        Task<PaginatedModel<ArchivedMediaModel>> GetMostBlockedMedia();

        Task<PaginatedModel<ArchivedMediaModel>> GetMostDislikedMedia();

        ArchivedMediaType? ResolveMediaType(string path);
    }
}