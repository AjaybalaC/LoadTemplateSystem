namespace LoadTemplateSystem.Web.Models.Entities;

public class Sku
{
    public int Id { get; set; }
    public string UID { get; set; } = Guid.NewGuid().ToString();
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public ICollection<SkuUom> Uoms { get; set; } = new List<SkuUom>();
}
