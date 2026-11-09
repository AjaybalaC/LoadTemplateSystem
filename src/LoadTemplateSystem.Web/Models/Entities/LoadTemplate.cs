namespace LoadTemplateSystem.Web.Models.Entities;

public class LoadTemplate
{
    public int Id { get; set; }
    public string UID { get; set; } = Guid.NewGuid().ToString();
    public string TemplateName { get; set; } = string.Empty;
    public string? TemplateDescription { get; set; }
    public int ItemCount { get; set; }

    public ICollection<LoadTemplateLine> Lines { get; set; } = new List<LoadTemplateLine>();
}
