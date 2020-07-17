using System.ComponentModel.DataAnnotations.Schema;

namespace Poncho.Models.Public
{
    public interface IHazPublicId
    {
        [NotMapped]
        PublicId PublicId { get; }
    }
}