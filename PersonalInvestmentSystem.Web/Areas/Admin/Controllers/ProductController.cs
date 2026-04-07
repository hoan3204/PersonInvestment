using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PersonalInvestmentSystem.Web.Domain.Entities;
using PersonalInvestmentSystem.Web.Domain.Enums;
using PersonalInvestmentSystem.Web.Services.Interfaces;
using PersonalInvestmentSystem.Web.UnitOfWork;
using PersonalInvestmentSystem.Web.ViewModels.Product;
using PersonalInvestmentSystem.Web.Domain.Entities;
using PersonalInvestmentSystem.Web.Domain.Enums;
using PersonalInvestmentSystem.Web.Services.Interfaces;
using PersonalInvestmentSystem.Web.UnitOfWork;
using Microsoft.Extensions.Configuration;

namespace PersonalInvestmentSystem.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ILogger<ProductController> _logger;
        private readonly IConfiguration _configuration;

        public ProductController(
            IProductService productService,
            IUnitOfWork unitOfWork,
            ICloudinaryService cloudinaryService,
            ILogger<ProductController> logger,
            IConfiguration configuration)
        {
            _productService = productService;
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
            _configuration = configuration;
        }

        // GET: Admin/Product
        public async Task<IActionResult> Index(int page = 1, string search = "")
        {
            var paged = await _productService.GetPagedProductsAsync(page, 10, search);
            ViewBag.Search = search;
            return View(paged);
        }

        // GET: Admin/Product/Create
        public async Task<IActionResult> Create()
        {
            await LoadDropdowns();
            return View(new InvestmentProduct());
        }

        // POST: Admin/Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InvestmentProduct model, IFormFile? ImageFile)
        {
            RemoveNavigationValidationErrors();

            if (!ModelState.IsValid)
            {
                await LoadDropdowns();
                return View(model);
            }

            try
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    _logger.LogInformation("[POST CREATE] Image file detected, size={Size}", ImageFile.Length);
                    var uploadedUrl = await _cloudinaryService.UploadImageAsync(ImageFile, "products");
                    if (!string.IsNullOrEmpty(uploadedUrl))
                    {
                        model.ImageUrl = uploadedUrl;
                        _logger.LogInformation("[POST CREATE] Image uploaded successfully");
                    }
                    else
                    {
                        _logger.LogWarning("[POST CREATE] Image upload returned null, continuing without image");
                    }
                }

                model.CreatedDate = DateTime.UtcNow;
                await _productService.AddProductAsync(model);

                TempData["Success"] = "Thêm sản phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create product failed. Code={Code}", model.Code);
                ModelState.AddModelError("", "Không thể thêm sản phẩm. Vui lòng thử lại.");
            }

            await LoadDropdowns();
            return View(model);
        }

        // GET: Admin/Product/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                _logger.LogInformation("Edit action called for product id={Id}", id);
                
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    _logger.LogWarning("Product not found for id={Id}", id);
                    return NotFound();
                }

                _logger.LogInformation("Loaded product: {Name} (ID={Id})", product.Name, product.Id);
                
                await LoadDropdowns();
                _logger.LogInformation("Dropdowns loaded successfully");
                
                _logger.LogInformation("Returning Edit view for product id={Id}", id);
                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[EDIT CRITICAL] Error in Edit GET action for id={Id}", id);
                Console.WriteLine($"[EDIT ERROR] {ex.Message}");
                Console.WriteLine($"[EDIT STACKTRACE] {ex.StackTrace}");
                throw;
            }
        }

        // POST: Admin/Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, InvestmentProduct model, IFormFile? ImageFile)
        {
            try
            {
                _logger.LogInformation("[POST EDIT] Starting Edit action for id={Id}", id);
                
                if (id != model.Id)
                {
                    _logger.LogWarning("[POST EDIT] ID mismatch: route id={RouteId}, model id={ModelId}", id, model.Id);
                    return NotFound();
                }

                RemoveNavigationValidationErrors();

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("[POST EDIT] ModelState invalid for id={Id}", id);
                    await LoadDropdowns();
                    return View(model);
                }

                _logger.LogInformation("[POST EDIT] Fetching existing product id={Id}", id);
                var existing = await _productService.GetProductByIdAsync(id);
                if (existing == null)
                {
                    _logger.LogWarning("[POST EDIT] Product not found for id={Id}", id);
                    return NotFound();
                }

                _logger.LogInformation("[POST EDIT] Product found: {Name}", existing.Name);

                // Upload ảnh mới nếu có
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    _logger.LogInformation("[POST EDIT] Image file detected for id={Id}, size={Size}", id, ImageFile.Length);
                    
                    // Xóa ảnh cũ trên Cloudinary nếu tồn tại
                    if (!string.IsNullOrEmpty(existing.ImageUrl))
                    {
                        _logger.LogInformation("[POST EDIT] Deleting old image for id={Id}", id);
                        await _cloudinaryService.DeleteImageAsync(existing.ImageUrl);
                    }
                    
                    _logger.LogInformation("[POST EDIT] Uploading new image for id={Id}", id);
                    var uploadedUrl = await _cloudinaryService.UploadImageAsync(ImageFile, "products");
                    if (!string.IsNullOrEmpty(uploadedUrl))
                    {
                        existing.ImageUrl = uploadedUrl;
                        _logger.LogInformation("[POST EDIT] Image uploaded successfully");
                    }
                    else
                    {
                        _logger.LogWarning("[POST EDIT] Image upload returned null, keeping existing image");
                    }
                }

                // Cập nhật thông tin
                _logger.LogInformation("[POST EDIT] Updating product properties for id={Id}", id);
                existing.Name = model.Name;
                existing.Code = model.Code;
                existing.Description = model.Description;
                existing.Type = model.Type;
                existing.CategoryId = model.CategoryId;
                existing.PublisherId = model.PublisherId;
                existing.CurrentPrice = model.CurrentPrice;
                existing.PreviousPrice = model.PreviousPrice;
                existing.ChangePercent = model.ChangePercent;
                existing.RiskLevel = model.RiskLevel;
                existing.IsFeatured = model.IsFeatured;
                existing.IsActive = model.IsActive;

                _logger.LogInformation("[POST EDIT] Saving product to database for id={Id}", id);
                await _productService.UpdateProductAsync(existing);
                _logger.LogInformation("[POST EDIT] Product saved successfully");

                _logger.LogInformation("[POST EDIT] Setting TempData success message");
                TempData["Success"] = "Cập nhật sản phẩm thành công!";
                
                _logger.LogInformation("[POST EDIT] About to RedirectToAction for id={Id}", id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[POST EDIT CRITICAL] Edit product failed. Id={Id}", id);
                Console.WriteLine($"[POST EDIT ERROR] {ex.Message}");
                Console.WriteLine($"[POST EDIT STACKTRACE] {ex.StackTrace}");
                ModelState.AddModelError("", "Không thể cập nhật sản phẩm. Vui lòng thử lại.");
            }

            await LoadDropdowns();
            return View(model);
        }

        // POST: Admin/Product/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _productService.DeleteProductAsync(id);
                TempData["Success"] = "Xóa sản phẩm thành công!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete product failed. Id={Id}", id);
                TempData["Error"] = "Không thể xóa sản phẩm.";
            }
            return RedirectToAction(nameof(Index));
        }

        //get/Admin/Product/Trash
        public async Task<IActionResult> Trash(int page = 1, string search = "")
        {
            var retentionDays = _configuration.GetValue<int>("ProductTrash:AutoDeleteAfterDays", 30);

            var removedCount = await _productService.AutoDeleteExpiredProductsAsync(retentionDays);

            if (retentionDays > 0)
            {
                TempData["Error"] = $"Đã tự động xóa vĩnh viễn {removedCount} sản phẩm quá hạn trong thùng rác.";

            }
            var paged = await _productService.GetPagedDeletedProductsAsync(page, 10, search);
            ViewBag.Search = search;
            ViewBag.RetentionDays = retentionDays;
            return View(paged);
        }
        //post Admin/Product/Restore/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            await _productService.RestoreProductAsync(id);
            TempData["Success"] = "Khôi phục sản phẩm thành công.";
            return RedirectToAction(nameof(Index));
        }
        //post Admin/Product/DeletePermanent/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePermanent(int id)
        {
            await _productService.PermanentlyDeleteProductAsync(id);
            TempData["Success"] = "Đã xóa vĩnh viễn.";
            return RedirectToAction(nameof(Trash));
        }

        private async Task LoadDropdowns()
        {
            try
            {
                _logger.LogInformation("LoadDropdowns: Starting to load categories...");
                var categories = await _unitOfWork.Categories.GetAllAsync();
                ViewBag.Categories = categories
                    .Where(c => c.IsActive)
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name });
                _logger.LogInformation("LoadDropdowns: Categories loaded - count={Count}", categories.Count());

                _logger.LogInformation("LoadDropdowns: Starting to load publishers...");
                var publishers = await _unitOfWork.Publishers.GetAllAsync();
                ViewBag.Publishers = publishers
                    .Where(p => p.IsActive)
                    .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name });
                _logger.LogInformation("LoadDropdowns: Publishers loaded - count={Count}", publishers.Count());

                _logger.LogInformation("LoadDropdowns: Loading enum types...");
                ViewBag.Types = Enum.GetValues(typeof(InvestmentType))
                    .Cast<InvestmentType>()
                    .Select(e => new SelectListItem { Value = ((int)e).ToString(), Text = e.ToString() });

                ViewBag.RiskLevels = Enum.GetValues(typeof(RiskLevel))
                    .Cast<RiskLevel>()
                    .Select(e => new SelectListItem { Value = ((int)e).ToString(), Text = e.ToString() });
                
                _logger.LogInformation("LoadDropdowns: All dropdowns loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CRITICAL] Error in LoadDropdowns");
                Console.WriteLine($"[LoadDropdowns ERROR] {ex.Message}");
                Console.WriteLine($"[LoadDropdowns STACKTRACE] {ex.StackTrace}");
                throw;
            }
        }

        private void RemoveNavigationValidationErrors()
        {
            ModelState.Remove("Category");
            ModelState.Remove("Publisher");
            ModelState.Remove("Transactions");
            ModelState.Remove("Reviews");
        }
    }
}