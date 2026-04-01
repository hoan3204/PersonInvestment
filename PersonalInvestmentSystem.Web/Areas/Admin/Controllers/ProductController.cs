using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public ProductController(IProductService productService, IUnitOfWork unitOfWork)
        {
            _productService = productService;
            _unitOfWork = unitOfWork;
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
        public async Task<IActionResult> Create(InvestmentProduct model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedDate = DateTime.UtcNow;
                await _productService.AddProductAsync(model);
                TempData["success"] = "Thêm sản phẩm thành công.";
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
        public async Task<IActionResult> Edit(int id, InvestmentProduct model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var existing = await _productService.GetProductByIdAsync(id);
                if (existing == null) return NotFound();

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
                .Select(c => new { c.Id, c.Name });
            ViewBag.Publishers = (await _unitOfWork.Publishers.GetAllAsync())
                .Where(p => p.IsActive)
                .Select(p => new { p.Id, p.Name });
            ViewBag.InvestmentTypes = Enum.GetValues(typeof(InvestmentType))
                .Cast<InvestmentType>()
                .Select(e => new { Value = (int)e, Text = e.ToString() });
            ViewBag.RiskLevels = Enum.GetValues(typeof(RiskLevel))
                .Cast<RiskLevel>()
                .Select(e => new { Value = (int)e, Text = e.ToString() });
        }
    }
}
