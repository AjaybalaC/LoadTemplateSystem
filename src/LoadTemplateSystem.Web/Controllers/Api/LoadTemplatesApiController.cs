using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoadTemplateSystem.Web.Data;
using LoadTemplateSystem.Web.Models.Api;
using LoadTemplateSystem.Web.Models.Entities;

namespace LoadTemplateSystem.Web.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Route("api/loadtemplates")]
[Route("api/load-templates")]
[Produces("application/json")]
public class LoadTemplatesApiController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<LoadTemplatesApiController> _logger;

    public LoadTemplatesApiController(AppDbContext context, ILogger<LoadTemplatesApiController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets a paginated list of Load Templates with search and sorting.
    /// </summary>
    /// <param name="searchTerm">Filter by Template Name (case-insensitive substring)</param>
    /// <param name="sortColumn">Column to sort by (TemplateName, Description, ItemCount, TotalUnits)</param>
    /// <param name="sortDirection">Sort direction (asc, desc)</param>
    /// <param name="page">Page number (1-based, default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 10, max: 100)</param>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<LoadTemplateSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<LoadTemplateSummaryDto>>>> GetAll(
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortColumn = "TemplateName",
        [FromQuery] string? sortDirection = "asc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.LoadTemplates.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(t => t.TemplateName.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync();

        var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        var orderedQuery = sortColumn?.ToLowerInvariant() switch
        {
            "description" => isDesc
                ? query.OrderByDescending(t => t.TemplateDescription)
                : query.OrderBy(t => t.TemplateDescription),
            "itemcount" => isDesc
                ? query.OrderByDescending(t => t.Lines.Any()
                    ? t.Lines.Sum(l => l.MonQty + l.TueQty + l.WedQty + l.ThuQty + l.FriQty + l.SatQty + l.SunQty)
                    : (decimal)t.ItemCount)
                : query.OrderBy(t => t.Lines.Any()
                    ? t.Lines.Sum(l => l.MonQty + l.TueQty + l.WedQty + l.ThuQty + l.FriQty + l.SatQty + l.SunQty)
                    : (decimal)t.ItemCount),
            "totalunits" => isDesc
                ? query.OrderByDescending(t => t.Lines.Sum(l => l.MonQty + l.TueQty + l.WedQty + l.ThuQty + l.FriQty + l.SatQty + l.SunQty))
                : query.OrderBy(t => t.Lines.Sum(l => l.MonQty + l.TueQty + l.WedQty + l.ThuQty + l.FriQty + l.SatQty + l.SunQty)),
            _ => isDesc
                ? query.OrderByDescending(t => t.TemplateName)
                : query.OrderBy(t => t.TemplateName)
        };

        var items = await orderedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new LoadTemplateSummaryDto
            {
                Id = t.Id,
                UID = t.UID,
                TemplateName = t.TemplateName,
                TemplateDescription = t.TemplateDescription,
                ItemCount = t.Lines.Any()
                    ? (int)Math.Round(t.Lines.Sum(l => l.MonQty + l.TueQty + l.WedQty + l.ThuQty + l.FriQty + l.SatQty + l.SunQty))
                    : t.ItemCount,
                TotalUnits = t.Lines.Any()
                    ? t.Lines.Sum(l => l.MonQty + l.TueQty + l.WedQty + l.ThuQty + l.FriQty + l.SatQty + l.SunQty)
                    : (decimal)t.ItemCount
            })
            .ToListAsync();

        var result = new PagedResponse<LoadTemplateSummaryDto>
        {
            CurrentPage = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = items
        };

        return Ok(ApiResponse<PagedResponse<LoadTemplateSummaryDto>>.Ok(result));
    }

    /// <summary>
    /// Gets a single Load Template by its ID, including all child lines.
    /// </summary>
    /// <param name="id">Template ID</param>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<LoadTemplateDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<LoadTemplateDetailDto>>> GetById(int id)
    {
        var template = await _context.LoadTemplates
            .AsNoTracking()
            .Include(t => t.Lines)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (template == null)
        {
            return NotFound(ApiResponse<LoadTemplateDetailDto>.Fail($"Load Template with ID #{id} was not found."));
        }

        var detailDto = await BuildDetailDtoAsync(template);
        return Ok(ApiResponse<LoadTemplateDetailDto>.Ok(detailDto));
    }

    /// <summary>
    /// Gets a single Load Template by its UID string, including all child lines.
    /// </summary>
    /// <param name="uid">Template UID</param>
    [HttpGet("by-uid/{uid}")]
    [ProducesResponseType(typeof(ApiResponse<LoadTemplateDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<LoadTemplateDetailDto>>> GetByUID(string uid)
    {
        var template = await _context.LoadTemplates
            .AsNoTracking()
            .Include(t => t.Lines)
            .FirstOrDefaultAsync(t => t.UID == uid);

        if (template == null)
        {
            return NotFound(ApiResponse<LoadTemplateDetailDto>.Fail($"Load Template with UID '{uid}' was not found."));
        }

        var detailDto = await BuildDetailDtoAsync(template);
        return Ok(ApiResponse<LoadTemplateDetailDto>.Ok(detailDto));
    }

    /// <summary>
    /// Creates a new Load Template with child lines.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<LoadTemplateDetailDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<LoadTemplateDetailDto>>> Create([FromBody] CreateLoadTemplateRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<LoadTemplateDetailDto>.Fail("Validation failed", errors));
        }

        var templateUid = string.IsNullOrWhiteSpace(request.UID)
            ? "LT-UID-" + Guid.NewGuid().ToString()[..8].ToUpper()
            : request.UID.Trim();

        // Check UID uniqueness
        if (await _context.LoadTemplates.AnyAsync(t => t.UID == templateUid))
        {
            return BadRequest(ApiResponse<LoadTemplateDetailDto>.Fail($"A template with UID '{templateUid}' already exists."));
        }

        var totalWeeklyUnits = request.Lines.Sum(l =>
            (l.MonQty ?? 0) + (l.TueQty ?? 0) + (l.WedQty ?? 0) +
            (l.ThuQty ?? 0) + (l.FriQty ?? 0) + (l.SatQty ?? 0) + (l.SunQty ?? 0));

        var entity = new LoadTemplate
        {
            UID = templateUid,
            TemplateName = request.TemplateName.Trim(),
            TemplateDescription = request.TemplateDescription?.Trim(),
            ItemCount = (int)Math.Round(totalWeeklyUnits)
        };

        foreach (var lineReq in request.Lines)
        {
            var skuExists = await _context.Skus.AnyAsync(s => s.Code == lineReq.SkuCode);
            if (!skuExists)
            {
                return BadRequest(ApiResponse<LoadTemplateDetailDto>.Fail($"SKU Code '{lineReq.SkuCode}' does not exist in the database."));
            }

            entity.Lines.Add(new LoadTemplateLine
            {
                UID = string.IsNullOrWhiteSpace(lineReq.UID) ? Guid.NewGuid().ToString() : lineReq.UID.Trim(),
                LoadTemplateUID = entity.UID,
                SKUCode = lineReq.SkuCode.Trim(),
                UOMCode = string.IsNullOrWhiteSpace(lineReq.UomCode) ? "EA" : lineReq.UomCode.Trim(),
                MonQty = Math.Max(0, lineReq.MonQty ?? 0),
                TueQty = Math.Max(0, lineReq.TueQty ?? 0),
                WedQty = Math.Max(0, lineReq.WedQty ?? 0),
                ThuQty = Math.Max(0, lineReq.ThuQty ?? 0),
                FriQty = Math.Max(0, lineReq.FriQty ?? 0),
                SatQty = Math.Max(0, lineReq.SatQty ?? 0),
                SunQty = Math.Max(0, lineReq.SunQty ?? 0)
            });
        }

        await _context.LoadTemplates.AddAsync(entity);
        await _context.SaveChangesAsync();

        var detailDto = await BuildDetailDtoAsync(entity);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, ApiResponse<LoadTemplateDetailDto>.Ok(detailDto, "Load Template created successfully."));
    }

    /// <summary>
    /// Updates an existing Load Template and its child lines.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<LoadTemplateDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<LoadTemplateDetailDto>>> Update(int id, [FromBody] UpdateLoadTemplateRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<LoadTemplateDetailDto>.Fail("Validation failed", errors));
        }

        var entity = await _context.LoadTemplates
            .Include(t => t.Lines)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (entity == null)
        {
            return NotFound(ApiResponse<LoadTemplateDetailDto>.Fail($"Load Template with ID #{id} was not found."));
        }

        if (!string.IsNullOrWhiteSpace(request.UID) && request.UID.Trim() != entity.UID)
        {
            var isTaken = await _context.LoadTemplates.AnyAsync(t => t.UID == request.UID.Trim() && t.Id != id);
            if (isTaken)
            {
                return BadRequest(ApiResponse<LoadTemplateDetailDto>.Fail($"UID '{request.UID}' is already used by another template."));
            }
            entity.UID = request.UID.Trim();
        }

        entity.TemplateName = request.TemplateName.Trim();
        entity.TemplateDescription = request.TemplateDescription?.Trim();

        // Process active lines
        var activeLines = request.Lines
            .Where(l => !l.IsDeleted && !string.IsNullOrWhiteSpace(l.SkuCode))
            .ToList();

        // Remove deleted lines
        var incomingUids = activeLines
            .Where(l => !string.IsNullOrWhiteSpace(l.UID))
            .Select(l => l.UID!)
            .ToHashSet();

        var toRemove = entity.Lines.Where(l => !incomingUids.Contains(l.UID)).ToList();
        foreach (var rem in toRemove)
        {
            _context.LoadTemplateLines.Remove(rem);
        }

        // Add or update lines
        foreach (var lineReq in activeLines)
        {
            var skuExists = await _context.Skus.AnyAsync(s => s.Code == lineReq.SkuCode);
            if (!skuExists)
            {
                return BadRequest(ApiResponse<LoadTemplateDetailDto>.Fail($"SKU Code '{lineReq.SkuCode}' does not exist in the database."));
            }

            var existingLine = entity.Lines.FirstOrDefault(l =>
                (!string.IsNullOrWhiteSpace(lineReq.UID) && l.UID == lineReq.UID) ||
                (lineReq.Id.HasValue && lineReq.Id.Value > 0 && l.Id == lineReq.Id.Value));

            if (existingLine != null)
            {
                existingLine.UOMCode = string.IsNullOrWhiteSpace(lineReq.UomCode) ? "EA" : lineReq.UomCode.Trim();
                existingLine.MonQty = Math.Max(0, lineReq.MonQty ?? 0);
                existingLine.TueQty = Math.Max(0, lineReq.TueQty ?? 0);
                existingLine.WedQty = Math.Max(0, lineReq.WedQty ?? 0);
                existingLine.ThuQty = Math.Max(0, lineReq.ThuQty ?? 0);
                existingLine.FriQty = Math.Max(0, lineReq.FriQty ?? 0);
                existingLine.SatQty = Math.Max(0, lineReq.SatQty ?? 0);
                existingLine.SunQty = Math.Max(0, lineReq.SunQty ?? 0);
            }
            else
            {
                entity.Lines.Add(new LoadTemplateLine
                {
                    UID = string.IsNullOrWhiteSpace(lineReq.UID) ? Guid.NewGuid().ToString() : lineReq.UID.Trim(),
                    LoadTemplateUID = entity.UID,
                    SKUCode = lineReq.SkuCode.Trim(),
                    UOMCode = string.IsNullOrWhiteSpace(lineReq.UomCode) ? "EA" : lineReq.UomCode.Trim(),
                    MonQty = Math.Max(0, lineReq.MonQty ?? 0),
                    TueQty = Math.Max(0, lineReq.TueQty ?? 0),
                    WedQty = Math.Max(0, lineReq.WedQty ?? 0),
                    ThuQty = Math.Max(0, lineReq.ThuQty ?? 0),
                    FriQty = Math.Max(0, lineReq.FriQty ?? 0),
                    SatQty = Math.Max(0, lineReq.SatQty ?? 0),
                    SunQty = Math.Max(0, lineReq.SunQty ?? 0)
                });
            }
        }

        var totalWeeklyUnits = activeLines.Sum(l =>
            (l.MonQty ?? 0) + (l.TueQty ?? 0) + (l.WedQty ?? 0) +
            (l.ThuQty ?? 0) + (l.FriQty ?? 0) + (l.SatQty ?? 0) + (l.SunQty ?? 0));
        entity.ItemCount = (int)Math.Round(totalWeeklyUnits);
        await _context.SaveChangesAsync();

        var detailDto = await BuildDetailDtoAsync(entity);
        return Ok(ApiResponse<LoadTemplateDetailDto>.Ok(detailDto, "Load Template updated successfully."));
    }

    /// <summary>
    /// Deletes a Load Template and its child lines.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var template = await _context.LoadTemplates.FindAsync(id);
        if (template == null)
        {
            return NotFound(ApiResponse<object>.Fail($"Load Template with ID #{id} was not found."));
        }

        _context.LoadTemplates.Remove(template);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { Id = id, template.TemplateName }, $"Load Template '{template.TemplateName}' deleted successfully."));
    }

    private async Task<LoadTemplateDetailDto> BuildDetailDtoAsync(LoadTemplate template)
    {
        var skuCodes = template.Lines.Select(l => l.SKUCode).Distinct().ToList();

        var skuNamesMap = await _context.Skus
            .Where(s => skuCodes.Contains(s.Code))
            .ToDictionaryAsync(s => s.Code, s => s.Name);

        var skuUomsMap = await _context.SkuUoms
            .Where(u => skuCodes.Contains(u.SkuCode))
            .GroupBy(u => u.SkuCode)
            .ToDictionaryAsync(
                g => g.Key,
                g => g.Select(u => new UomOptionDto { Code = u.Code, Name = u.Name }).ToList()
            );

        var uomNameMap = await _context.SkuUoms
            .Where(u => skuCodes.Contains(u.SkuCode))
            .ToDictionaryAsync(u => $"{u.SkuCode}_{u.Code}", u => u.Name);

        var lineDtos = template.Lines.Select(l =>
        {
            skuNamesMap.TryGetValue(l.SKUCode, out var skuName);
            skuUomsMap.TryGetValue(l.SKUCode, out var availableUoms);
            uomNameMap.TryGetValue($"{l.SKUCode}_{l.UOMCode}", out var uomName);

            return new LoadTemplateLineDto
            {
                Id = l.Id,
                UID = l.UID,
                LoadTemplateUID = l.LoadTemplateUID,
                SkuCode = l.SKUCode,
                SkuName = skuName ?? l.SKUCode,
                UomCode = l.UOMCode,
                UomName = uomName ?? l.UOMCode,
                MonQty = l.MonQty,
                TueQty = l.TueQty,
                WedQty = l.WedQty,
                ThuQty = l.ThuQty,
                FriQty = l.FriQty,
                SatQty = l.SatQty,
                SunQty = l.SunQty,
                AvailableUoms = availableUoms ?? new List<UomOptionDto>()
            };
        }).ToList();

        return new LoadTemplateDetailDto
        {
            Id = template.Id,
            UID = template.UID,
            TemplateName = template.TemplateName,
            TemplateDescription = template.TemplateDescription,
            ItemCount = lineDtos.Count,
            TotalUnits = lineDtos.Sum(l => l.TotalLineQty),
            Lines = lineDtos
        };
    }
}

