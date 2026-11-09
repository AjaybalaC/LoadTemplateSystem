using System.ComponentModel.DataAnnotations;

namespace LoadTemplateSystem.Web.Models.Api;

public class SkuDto
{
    public int Id { get; set; }
    public string UID { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<UomOptionDto> Uoms { get; set; } = new();
}

public class SkuUomDto
{
    public int Id { get; set; }
    public string UID { get; set; } = string.Empty;
    public string SkuCode { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class UomOptionDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class CreateSkuRequest
{
    [StringLength(250)]
    public string? UID { get; set; }

    [Required(ErrorMessage = "SKU Code is required")]
    [StringLength(250)]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "SKU Name is required")]
    [StringLength(250)]
    public string Name { get; set; } = string.Empty;

    public List<CreateSkuUomRequest>? Uoms { get; set; }
}

public class CreateSkuUomRequest
{
    [StringLength(250)]
    public string? UID { get; set; }

    [Required(ErrorMessage = "UOM Code is required")]
    [StringLength(250)]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "UOM Name is required")]
    [StringLength(250)]
    public string Name { get; set; } = string.Empty;
}

