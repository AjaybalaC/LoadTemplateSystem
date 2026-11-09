namespace LoadTemplateSystem.Web.Models.ViewModels;

public class LoadTemplateListVm
{
    public string? SearchTerm { get; set; }
    public string SortColumn { get; set; } = "TemplateName";
    public string SortDirection { get; set; } = "asc";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalItems { get; set; }

    public int TotalPages => Math.Max(1, (int)Math.Ceiling((double)TotalItems / PageSize));
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public List<LoadTemplateSummaryVm> Templates { get; set; } = new();
}

public class LoadTemplateSummaryVm
{
    public int Id { get; set; }
    public string UID { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string? TemplateDescription { get; set; }
    public int ItemCount { get; set; }
    public decimal TotalUnits { get; set; }
}
