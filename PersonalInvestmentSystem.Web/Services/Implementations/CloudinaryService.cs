using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using iText.Forms.Form.Element;
using Microsoft.Extensions.Options;
using PersonalInvestmentSystem.Web.Models;
using PersonalInvestmentSystem.Web.Services.Interfaces;

namespace PersonalInvestmentSystem.Web.Services.Implementations
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryService> _logger;

        public CloudinaryService(IOptions<CloudinarySettings> config, ILogger<CloudinaryService> logger)
        {
            _logger = logger;
            
            try
            {
                // Validate Cloudinary config
                if (string.IsNullOrEmpty(config.Value.CloudName) || 
                    string.IsNullOrEmpty(config.Value.ApiKey) || 
                    string.IsNullOrEmpty(config.Value.ApiSecret))
                {
                    _logger.LogWarning("Cloudinary credentials are missing. Image upload will fail.");
                }

                var account = new Account(
                    config.Value.CloudName,
                    config.Value.ApiKey,
                    config.Value.ApiSecret);
                _cloudinary = new Cloudinary(account);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Cloudinary service");
                throw;
            }
        }

        public async Task<string> UploadImageAsync(IFormFile file, string? folder = "products")
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("UploadImageAsync called with null or empty file");
                    return null;
                }

                _logger.LogInformation("UploadImageAsync: Starting upload for file {FileName}, size={Size}", file.FileName, file.Length);

                using var stream = file.OpenReadStream();
                _logger.LogInformation("UploadImageAsync: Stream opened successfully");
                
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = folder,
                    Transformation = new Transformation().Width(800).Height(800).Crop("limit")
                };

                _logger.LogInformation("UploadImageAsync: Calling Cloudinary API...");
                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                
                _logger.LogInformation("UploadImageAsync: Cloudinary response received");
                
                if (uploadResult.Error != null)
                {
                    _logger.LogError("Cloudinary upload error: {Error}", uploadResult.Error.Message);
                    return null;
                }

                var url = uploadResult.SecureUrl?.ToString();
                _logger.LogInformation("UploadImageAsync: Upload successful, URL={URL}", url);
                return url;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UploadImageAsync: CRITICAL ERROR - Error uploading image to Cloudinary");
                Console.WriteLine($"[IMAGE UPLOAD CRITICAL] {ex.GetType().Name}: {ex.Message}");
                Console.WriteLine($"[IMAGE UPLOAD STACKTRACE] {ex.StackTrace}");
                return null; // Return null instead of throwing - allow product update without image
            }
            finally
            {
                _logger.LogInformation("UploadImageAsync: Cleanup complete");
            }
        }

        public async Task<bool> DeleteImageAsync(string publicId)
        {
            try
            {
                if (string.IsNullOrEmpty(publicId)) return false;
                
                var deletionParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deletionParams);
                return result.Result == "ok";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete image from Cloudinary. PublicId={PublicId}", publicId);
                return false; // Don't throw, just log warning
            }
        }
    } 
}
