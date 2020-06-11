namespace seattle.Services
{
    public class ImageData
    {
        public byte[] data { get; set; }
    }

    public interface IMIPMapService
    {
        int AddImage(ImageData image);
        ImageData GetMIPMap(int id, int resolution);
        void DeleteImage(int id);
    }
}