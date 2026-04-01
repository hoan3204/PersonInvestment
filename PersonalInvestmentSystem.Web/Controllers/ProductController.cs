using Microsoft.AspNetCore.Mvc;
using PersonalInvestmentSystem.Web.Services.Interfaces;

namespace PersonalInvestmentSystem.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        //danh sach san pham
        public async Task<IActionResult> Index(int page =1, string search = "")
        {
            var paged = await _productService.GetPagedProductsAsync(page, 12, search);
            ViewBag.Search = search;
            return View(paged);
        }

        //chi tiet san pham
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null) return NotFound();

            ViewBag.PriceHistory = new List<object>
            {
                new {Date = "25/3", Price = product.PreviousPrice * 0.97m},
                new { Date = "26/03", Price = product.PreviousPrice * 0.98m },
                new { Date = "27/03", Price = product.PreviousPrice * 0.99m },
                new { Date = "28/03", Price = product.PreviousPrice },
                new { Date = "29/03", Price = product.CurrentPrice * 0.995m },
                new { Date = "30/03", Price = product.CurrentPrice * 0.998m },
                new { Date = "31/03", Price = product.CurrentPrice }
            };
            return View(product);

        }
        public async Task<IActionResult> Featured()
        {
            var producs = await _productService.GetFeaturedProductsAsync(8);
            return PartialView("_FeaturedProducts", producs);
        }
    }
}
