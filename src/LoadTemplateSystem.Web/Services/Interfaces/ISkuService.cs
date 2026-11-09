using LoadTemplateSystem.Web.Models.ViewModels;

namespace LoadTemplateSystem.Web.Services.Interfaces;

public interface ISkuService
{
    Task<List<ProductSearchItemVm>> SearchProductsAsync(string? query, int maxResults = 25);
    Task<List<UomOptionVm>> GetUomsForSkuCodeAsync(string skuCode);
    Task<LoadTemplateLineVm?> CreateLineForSkuCodeAsync(string skuCode, string templateUid, int sequence = 0);
}
