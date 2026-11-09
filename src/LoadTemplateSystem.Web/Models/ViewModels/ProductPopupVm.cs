namespace LoadTemplateSystem.Web.Models.ViewModels;

public class ProductPopupVm
{
    public string? SearchTerm { get; set; }
    public List<ProductSearchItemVm> Products { get; set; } = new();
}

public class ProductSearchItemVm
{
    public int Id { get; set; }
    public string UID { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DefaultUomCode { get; set; } = "EA";
    public List<UomOptionVm> AvailableUoms { get; set; } = new();
}
