using System.ComponentModel.DataAnnotations.Schema;

namespace PinkUmbrella.Models.Public
{
    public interface IHazPublicId
    {
        [NotMapped]
        PublicId PublicId { get; }
    }
}