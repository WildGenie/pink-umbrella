namespace PinkUmbrella.Models.Public
{
    public class PublicModel<T> where T: IHazPublicId
    {
        public PublicId Id => Local?.PublicId;

        public T Local { get; set; }
    }
}