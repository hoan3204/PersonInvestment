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
                    model.ImageUrl = await _cloudinaryService.UploadImageAsync(ImageFile, "products");
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
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null) return NotFound();

            await LoadDropdowns();
            return View(product);
        }

        // POST: Admin/Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, InvestmentProduct model, IFormFile? ImageFile)
        {
            if (id != model.Id) return NotFound();

            RemoveNavigationValidationErrors();

            if (!ModelState.IsValid)
            {
                await LoadDropdowns();
                return View(model);
            }

            try
            {
                var existing = await _productService.GetProductByIdAsync(id);
                if (existing == null) return NotFound();

                // Upload ảnh mới nếu có
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    // Xóa ảnh cũ trên Cloudinary nếu tồn tại
                    if (!string.IsNullOrEmpty(existing.ImageUrl))
                    {
                        await _cloudinaryService.DeleteImageAsync(existing.ImageUrl);
                    }
                    existing.ImageUrl = await _cloudinaryService.UploadImageAsync(ImageFile, "products");
                }

                // Cập nhật thông tin
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

                await _productService.UpdateProductAsync(existing);

                TempData["Success"] = "Cập nhật sản phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Edit product failed. Id={Id}", id);
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
            ViewBag.Categories = (await _unitOfWork.Categories.GetAllAsync())
                .Where(c => c.IsActive)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name });

            ViewBag.Publishers = (await _unitOfWork.Publishers.GetAllAsync())
                .Where(p => p.IsActive)
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name });

            ViewBag.Types = Enum.GetValues(typeof(InvestmentType))
                .Cast<InvestmentType>()
                .Select(e => new SelectListItem { Value = ((int)e).ToString(), Text = e.ToString() });

            ViewBag.RiskLevels = Enum.GetValues(typeof(RiskLevel))
                .Cast<RiskLevel>()
                .Select(e => new SelectListItem { Value = ((int)e).ToString(), Text = e.ToString() });
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