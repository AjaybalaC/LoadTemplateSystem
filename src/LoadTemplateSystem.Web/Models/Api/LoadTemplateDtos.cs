using System.ComponentModel.DataAnnotations;

namespace LoadTemplateSystem.Web.Models.Api;

public class LoadTemplateSummaryDto
{
    public int Id { get; set; }
    public string UID { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string? TemplateDescription { get; set; }
    public int ItemCount { get; set; }
    public decimal TotalUnits { get; set; }
}

public class LoadTemplateDetailDto
{
    public int Id { get; set; }
    public string UID { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string? TemplateDescription { get; set; }
    public int ItemCount { get; set; }
    public decimal TotalUnits { get; set; }
    public List<LoadTemplateLineDto> Lines { get; set; } = new();
}

public class LoadTemplateLineDto
{
    public int Id { get; set; }
    public string UID { get; set; } = string.Empty;
    public string LoadTemplateUID { get; set; } = string.Empty;
    public string SkuCode { get; set; } = string.Empty;
    public string SkuName { get; set; } = string.Empty;
    public string UomCode { get; set; } = string.Empty;
    public string UomName { get; set; } = string.Empty;
    public decimal MonQty { get; set; }
    public decimal TueQty { get; set; }
    public decimal WedQty { get; set; }
    public decimal ThuQty { get; set; }
    public decimal FriQty { get; set; }
    public decimal SatQty { get; set; }
    public decimal SunQty { get; set; }
    public decimal TotalLineQty => MonQty + TueQty + WedQty + ThuQty + FriQty + SatQty + SunQty;
    public List<UomOptionDto> AvailableUoms { get; set; } = new();
}

public class CreateLoadTemplateRequest
{
    [StringLength(250)]
    public string? UID { get; set; }

    [Required(ErrorMessage = "Template Name is required")]
    [StringLength(250, ErrorMessage = "Template Name cannot exceed 250 characters")]
    public string TemplateName { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "Template Description cannot exceed 250 characters")]
    public string? TemplateDescription { get; set; }

    public List<CreateLoadTemplateLineRequest> Lines { get; set; } = new();
}

public class CreateLoadTemplateLineRequest
{
    public string? UID { get; set; }

    [Required(ErrorMessage = "SKU Code is required")]
    [StringLength(250)]
    public string SkuCode { get; set; } = string.Empty;

    [StringLength(250)]
    public string UomCode { get; set; } = "EA";

    [Range(0, 9999999999999999.99)]
    public decimal? MonQty { get; set; }

    [Range(0, 9999999999999999.99)]
    public decimal? TueQty { get; set; }

    [Range(0, 9999999999999999.99)]
    public decimal? WedQty { get; set; }

    [Range(0, 9999999999999999.99)]
    public decimal? ThuQty { get; set; }

    [Range(0, 9999999999999999.99)]
    public decimal? FriQty { get; set; }

    [Range(0, 9999999999999999.99)]
    public decimal? SatQty { get; set; }

    [Range(0, 9999999999999999.99)]
    public decimal? SunQty { get; set; }
}

public class UpdateLoadTemplateRequest
{
    [StringLength(250)]
    public string? UID { get; set; }

    [Required(ErrorMessage = "Template Name is required")]
    [StringLength(250, ErrorMessage = "Template Name cannot exceed 250 characters")]
    public string TemplateName { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "Template Description cannot exceed 250 characters")]
    public string? TemplateDescription { get; set; }

    public List<UpdateLoadTemplateLineRequest> Lines { get; set; } = new();
}

public class UpdateLoadTemplateLineRequest
{
    public int? Id { get; set; }
    public string? UID { get; set; }

    [Required(ErrorMessage = "SKU Code is required")]
    [StringLength(250)]
    public string SkuCode { get; set; } = string.Empty;

    [StringLength(250)]
    public string UomCode { get; set; } = "EA";

    [Range(0, 9999999999999999.99)]
    public decimal? MonQty { get; set; }

    [Range(0, 9999999999999999.99)]
    public decimal? TueQty { get; set; }

    [Range(0, 9999999999999999.99)]
    public decimal? WedQty { get; set; }

    [Range(0, 9999999999999999.99)]
    public decimal? ThuQty { get; set; }

    [Range(0, 9999999999999999.99)]
    public decimal? FriQty { get; set; }

    [Range(0, 9999999999999999.99)]
    public decimal? SatQty { get; set; }

    [Range(0, 9999999999999999.99)]
    public decimal? SunQty { get; set; }

    public bool IsDeleted { get; set; }
}

public class PagedResponse<T>
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
    public List<T> Items { get; set; } = new();
}

