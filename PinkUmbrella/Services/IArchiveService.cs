using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using PinkUmbrella.Models;
using Estuary.Core;
using Tides.Models;
using Tides.Models.Public;
using Estuary.Objects;

namespace PinkUmbrella.Services
{
    public interface IArchiveService
    {
        Task<NewMediaResult> UploadMedia(BaseObject media);


        Task<BaseObject> GetMedia(PublicId id, int? viewerId);

        Task<BaseObject> GetMedia(string path, int? viewerId);
        
        Task<CollectionObject> GetMediaForUser(PublicId userId, int? viewerId, CommonMediaType? type, PaginationModel pagination);
        
        Task<BaseObject> DeleteMedia(int id, int? viewerId);
        
        Task<MediaScanResultModel> ScanMediaForProfanity(BaseObject media, int? viewerId);
        
        Task<MediaScanResultModel> ScanMediaForVirusOrBadThings(BaseObject media, int? viewerId);
        
        Task<MediaScanResultModel> ScanMediaForKKKops(BaseObject media, int? viewerId);

        Task<Stream> GetStream(BaseObject media, int? viewerId);


        // Task<CollectionObject> GetMostReportedMedia();
        
        // Task<CollectionObject> GetMostBlockedMedia();

        // Task<CollectionObject> GetMostDislikedMedia();

        CommonMediaType? ResolveMediaType(string path);
        
        string LocalPath(BaseObject doc);

        Task<BaseObject> Transform(ArchivedMediaModel archivedMediaModel);
        
        Task CopyUpload(ModelStateDictionary modelState, List<IFormFile> files, List<BaseObject> fileModels);
        
        Task<List<BaseObject>> GenModels(ModelStateDictionary modelState, List<IFormFile> files, string description, string title, string attribution, int? relatedPostId, Visibility visibility);
    }
}