using System.ComponentModel.DataAnnotations;

namespace LoadTemplateSystem.Web.Models.ViewModels;

public class LoadTemplateEditVm
{
    public int Id { get; set; }

    [Display(Name = "UID")]
    public string UID { get; set; } = Guid.NewGuid().ToString();

    [Required(ErrorMessage = "Template Name is required")]
    [StringLength(250, ErrorMessage = "Template Name cannot exceed 250 characters")]
    [Display(Name = "Template Name")]
    public string TemplateName { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "Template Description cannot exceed 250 characters")]
    [Display(Name = "Template Description")]
    public string? TemplateDescription { get; set; }

    [Display(Name = "Item Count")]
    public int ItemCount => (int)Math.Round(OverallTotalQty);

    public List<LoadTemplateLineVm> Lines { get; set; } = new();

    public decimal OverallTotalQty => Lines.Where(l => !l.IsDeleted).Sum(l => l.TotalQty);
}
