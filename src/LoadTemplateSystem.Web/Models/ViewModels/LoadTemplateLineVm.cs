namespace LoadTemplateSystem.Web.Models.ViewModels;

public class LoadTemplateLineVm
{
    public int Id { get; set; }
    public string UID { get; set; } = Guid.NewGuid().ToString();
    public string LoadTemplateUID { get; set; } = string.Empty;

    public string SkuCode { get; set; } = string.Empty;
    public string SkuName { get; set; } = string.Empty;

    // Note: While showing UOM in dropdown -> show Name, but while saving in LoadTemplateLine -> save Code.
    public string SelectedUomCode { get; set; } = "EA";
    public List<UomOptionVm> AvailableUoms { get; set; } = new();

    public decimal? MonQty { get; set; }
    public decimal? TueQty { get; set; }
    public decimal? WedQty { get; set; }
    public decimal? ThuQty { get; set; }
    public decimal? FriQty { get; set; }
    public decimal? SatQty { get; set; }
    public decimal? SunQty { get; set; }

    public decimal TotalQty => (MonQty ?? 0) + (TueQty ?? 0) + (WedQty ?? 0) + (ThuQty ?? 0) + (FriQty ?? 0) + (SatQty ?? 0) + (SunQty ?? 0);
    public bool IsDeleted { get; set; }
    public int Sequence { get; set; }
}

public class UomOptionVm
{
    public string Code { get; set; } = string.Empty; // Saved to LoadTemplateLine.UOMCode
    public string Name { get; set; } = string.Empty; // Displayed in Dropdown
}
