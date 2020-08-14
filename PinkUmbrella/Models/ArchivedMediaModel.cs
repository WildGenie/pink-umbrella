using System.ComponentModel;
using Estuary.Objects;

namespace PinkUmbrella.Models
{
    public class ArchivedMediaModel
    {
        public long Id { get; set; }
        public int UserId { get; set; }
        public long SizeBytes { get; set; }
        public CommonMediaType MediaType { get; set; }

        [DefaultValue(false)]
        public bool UploadedStatus { get; set; }
    }
}