using Microsoft.AspNetCore.Mvc;
using LoadTemplateSystem.Web.Services.Interfaces;

namespace LoadTemplateSystem.Web.Controllers;

public class ProductController : Controller
{
    private readonly ISkuService _skuService;

    public ProductController(ISkuService skuService)
    {
        _skuService = skuService;
    }

    // GET: Product/Search?query=milk
    // Popup search (key-press AJAX)
    [HttpGet]
    public async Task<IActionResult> Search(string? query)
    {
        var products = await _skuService.SearchProductsAsync(query, maxResults: 25);
        return Json(new
        {
            success = true,
            total = products.Count,
            data = products
        });
    }

    // GET: Product/GetUoms?skuCode=SKU-1001
    [HttpGet]
    public async Task<IActionResult> GetUoms(string skuCode)
    {
        var uoms = await _skuService.GetUomsForSkuCodeAsync(skuCode);
        return Json(new { success = true, uoms });
    }
}
