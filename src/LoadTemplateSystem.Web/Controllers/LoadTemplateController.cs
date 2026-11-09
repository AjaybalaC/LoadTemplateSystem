using Microsoft.AspNetCore.Mvc;
using LoadTemplateSystem.Web.Models.ViewModels;
using LoadTemplateSystem.Web.Services.Interfaces;

namespace LoadTemplateSystem.Web.Controllers;

public class LoadTemplateController : Controller
{
    private readonly ILoadTemplateService _loadTemplateService;
    private readonly ISkuService _skuService;
    private readonly ILogger<LoadTemplateController> _logger;

    public LoadTemplateController(
        ILoadTemplateService loadTemplateService,
        ISkuService skuService,
        ILogger<LoadTemplateController> logger)
    {
        _loadTemplateService = loadTemplateService;
        _skuService = skuService;
        _logger = logger;
    }

    // GET: LoadTemplate
    // Maintain Load Request Template (sort / page / search)
    [HttpGet]
    public async Task<IActionResult> Index(
        string? searchTerm,
        string? sortColumn,
        string? sortDirection,
        int page = 1,
        int pageSize = 10)
    {
        var model = await _loadTemplateService.GetTemplatesPagedAsync(
            searchTerm, sortColumn, sortDirection, page, pageSize);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return PartialView("_TemplateListPartial", model);
        }

        return View(model);
    }

    // GET: LoadTemplate/Create
    [HttpGet]
    public IActionResult Create()
    {
        var model = new LoadTemplateEditVm
        {
            UID = "LT-UID-" + Guid.NewGuid().ToString()[..8].ToUpper(),
            Lines = new List<LoadTemplateLineVm>()
        };

        ViewBag.Title = "Create Load Request Template";
        ViewBag.IsNew = true;
        return View("Edit", model);
    }

    // GET: LoadTemplate/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _loadTemplateService.GetTemplateForEditAsync(id);
        if (model == null)
        {
            TempData["ErrorMessage"] = $"Load Template with ID #{id} was not found.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Title = $"Edit Load Request Template - {model.TemplateName}";
        ViewBag.IsNew = false;
        return View(model);
    }

    // POST: LoadTemplate/Save
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(LoadTemplateEditVm model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Title = model.Id == 0 ? "Create Load Request Template" : $"Edit Load Request Template - {model.TemplateName}";
            ViewBag.IsNew = model.Id == 0;
            return View("Edit", model);
        }

        try
        {
            var savedId = await _loadTemplateService.SaveTemplateAsync(model);
            TempData["SuccessMessage"] = model.Id == 0
                ? $"Load Template '{model.TemplateName}' (UID: {model.UID}) created successfully."
                : $"Load Template '{model.TemplateName}' (UID: {model.UID}) updated successfully.";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving Load Template");
            ModelState.AddModelError(string.Empty, "An error occurred while saving the template: " + ex.Message);
            ViewBag.Title = model.Id == 0 ? "Create Load Request Template" : $"Edit Load Request Template - {model.TemplateName}";
            ViewBag.IsNew = model.Id == 0;
            return View("Edit", model);
        }
    }

    // POST: LoadTemplate/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _loadTemplateService.DeleteTemplateAsync(id);
        if (success)
        {
            TempData["SuccessMessage"] = "Load Template deleted successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to delete Load Template. It may not exist.";
        }

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Json(new { success });
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: LoadTemplate/GetLineRow?skuCode=SKU-1001&templateUid=LT-UID-1&index=0
    [HttpGet]
    public async Task<IActionResult> GetLineRow(string skuCode, string templateUid, int index)
    {
        var lineVm = await _skuService.CreateLineForSkuCodeAsync(skuCode, templateUid, index + 1);
        if (lineVm == null)
        {
            return NotFound("Product not found.");
        }

        ViewData["RowIndex"] = index;
        return PartialView("_LineRow", lineVm);
    }
}
