namespace PersonalInvestmentSystem.Web.Services.Interfaces
{
    public interface ICloudinaryService
    {
        Task<string> UploadImageAsync(IFormFile file, string? folder = "products");
        Task<bool> DeleteImageAsync(string publicId);
    }
}
