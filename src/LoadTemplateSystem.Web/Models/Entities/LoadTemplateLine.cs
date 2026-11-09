namespace LoadTemplateSystem.Web.Models.Entities;

public class LoadTemplateLine
{
    public int Id { get; set; }
    public string UID { get; set; } = Guid.NewGuid().ToString();
    public string LoadTemplateUID { get; set; } = string.Empty;
    public LoadTemplate? LoadTemplate { get; set; }

    public string SKUCode { get; set; } = string.Empty;
    public string UOMCode { get; set; } = string.Empty;

    public decimal MonQty { get; set; }
    public decimal TueQty { get; set; }
    public decimal WedQty { get; set; }
    public decimal ThuQty { get; set; }
    public decimal FriQty { get; set; }
    public decimal SatQty { get; set; }
    public decimal SunQty { get; set; }

    public decimal TotalQty => MonQty + TueQty + WedQty + ThuQty + FriQty + SatQty + SunQty;
}
