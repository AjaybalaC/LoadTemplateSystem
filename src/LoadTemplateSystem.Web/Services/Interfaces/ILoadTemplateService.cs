using LoadTemplateSystem.Web.Models.ViewModels;

namespace LoadTemplateSystem.Web.Services.Interfaces;

public interface ILoadTemplateService
{
    Task<LoadTemplateListVm> GetTemplatesPagedAsync(string? searchTerm, string? sortColumn, string? sortDirection, int pageNumber, int pageSize);
    Task<LoadTemplateEditVm?> GetTemplateForEditAsync(int id);
    Task<LoadTemplateEditVm?> GetTemplateForEditByUIDAsync(string uid);
    Task<int> SaveTemplateAsync(LoadTemplateEditVm model);
    Task<bool> DeleteTemplateAsync(int id);
}
