using WebApplication1.Services.Random;

namespace WebApplication1.Services.Storage
{
    public class LocalStorageService(IRandomService randomService) : IStorageService
    {
        const String StoragePath = "Storage/";
        private readonly IRandomService _randomService = randomService;
        public string Save(IFormFile formFile)
        {
            ArgumentNullException.ThrowIfNull(formFile);
            if (formFile.Length == 0)
            {
                throw new InvalidDataException("Empty File");
            }
            String ext = Path.GetExtension(formFile.FileName);
            String savedName = _randomService.FileName() + ext;
            String fullPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                StoragePath + savedName
            );
            using (FileStream fileStream = new(fullPath, FileMode.CreateNew))
            {
                formFile.CopyTo(fileStream);
            }
            return savedName;
        }
        public bool Delete(string fileUrl)
        {
            throw new NotImplementedException();
        }
        public Stream? Get(string fileUrl)
        {
            String fullPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                StoragePath + fileUrl
            );
            if (File.Exists(fullPath))
            {
                return new FileStream(fullPath, FileMode.Open);
            }
            return null;
        }
    }
}
