using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PersonalInvestmentSystem.Web.Services.Interfaces;
using PersonalInvestmentSystem.Web.UnitOfWork;
using PersonalInvestmentSystem.Web.Domain.Entities;
using PersonalInvestmentSystem.Web.Domain.Enums;
using PersonalInvestmentSystem.Web.ViewModels.Product;


namespace PersonalInvestmentSystem.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles ="Admin")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IProductService productService, IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _productService = productService;
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        //get Admin/Product
        public async Task<IActionResult> Index(int page =1 , string search = "")
        {
            var paged = await _productService.GetPagedProductsAsync(page, 10, search);
            ViewBag.search = search;
            return View(paged);
        }

        // get Admin/Product/Create
        public async Task<IActionResult> Create()
        {
            await LoadDropdowns();
            return View(new InvestmentProduct());
        }

        //post Admin/Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InvestmentProduct model, IFormFile? ImageFile)
        {
            RemoveNavigationValidationErrors();
            if (ModelState.IsValid)
            {
                if (ImageFile != null)
                {
                    model.ImageUrl = await UploadImageAsync(ImageFile);
                }
                model.CreatedDate = DateTime.UtcNow;
                await _productService.AddProductAsync(model);
                TempData["Success"] = "Thêm sản phẩm thành công.";
                return RedirectToAction(nameof(Index));
            }
            await LoadDropdowns();
            return View(model);
        }

        //get Admin/Product/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null) return NotFound();

            await LoadDropdowns();
            return View(product);
        }

        //get Admin/Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, InvestmentProduct model, IFormFile? ImageFile)
        {
            RemoveNavigationValidationErrors();
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var existing = await _productService.GetProductByIdAsync(id);
                if (existing == null) return NotFound();

                if (ImageFile != null && ImageFile.Length > 0)
                {
                    existing.ImageUrl = await UploadImageAsync(ImageFile);
                }
                //cap nhat cac truong
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
                existing.ImageUrl = model.ImageUrl;
                existing.IsFeatured = model.IsFeatured;
                existing.IsActive = model.IsActive;

                await _productService.UpdateProductAsync(existing);
                TempData["Success"] = "Cập nhật sản phẩm thành công.";
                return RedirectToAction(nameof(Index));

            }
            await LoadDropdowns();
            return View(model);
        }

        //post Admin/Product/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _productService.DeleteProductAsync(id);
            TempData["Success"] = "Xóa thành công.";
            return RedirectToAction(nameof(Index));
        }
        private async Task LoadDropdowns()
        {
            ViewBag.Categories = (await _unitOfWork.Categories.GetAllAsync())
                .Where(c => c.IsActive)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                });

            ViewBag.Publishers = (await _unitOfWork.Publishers.GetAllAsync())
                .Where(p => p.IsActive)
                .Select(p => new SelectListItem
                { 
                    Value = p.Id.ToString(),
                    Text = p.Name
                })
                .ToList();

            ViewBag.Types = Enum.GetValues(typeof(InvestmentType))
                .Cast<InvestmentType>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = e.ToString()
                })
                .ToList();
            ViewBag.RiskLevels = Enum.GetValues(typeof(RiskLevel))
                .Cast<RiskLevel>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = e.ToString()
                })
                .ToList();
        }

        //them anh
        public async Task<string?> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0) return null;

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return "/images/products" + uniqueFileName;
        }
        private void RemoveNavigationValidationErrors()
        {
            ModelState.Remove(nameof(InvestmentProduct.Category));
            ModelState.Remove(nameof(InvestmentProduct.Publisher));
            ModelState.Remove(nameof(InvestmentProduct.Transactions));
            ModelState.Remove(nameof(InvestmentProduct.Reviews));
        }
    }
}
