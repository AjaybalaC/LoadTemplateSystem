using System.ComponentModel.DataAnnotations.Schema;

namespace LoadTemplateSystem.Web.Models.Entities;

public class SkuUom
{
    public int Id { get; set; }
    public string UID { get; set; } = Guid.NewGuid().ToString();

    [Column("SKU Code")]
    public string SkuCode { get; set; } = string.Empty;
    public Sku? Sku { get; set; }

    public string Code { get; set; } = string.Empty; // e.g. EA, OU, CT (saved in LoadTemplateLine.UOMCode)
    public string Name { get; set; } = string.Empty; // e.g. Each, Outer, Carton (shown in dropdown)
}
