using System.ComponentModel.DataAnnotations.Schema;

namespace Tides.Models.Public
{
    public interface IHazPublicId
    {
        [NotMapped]
        PublicId PublicId { get; }
    }
}