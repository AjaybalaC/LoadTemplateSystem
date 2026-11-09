using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoadTemplateSystem.Web.Data;
using LoadTemplateSystem.Web.Models.Api;
using LoadTemplateSystem.Web.Models.Entities;

namespace LoadTemplateSystem.Web.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Route("api/skus")]
[Produces("application/json")]
public class SkusApiController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<SkusApiController> _logger;

    public SkusApiController(AppDbContext context, ILogger<SkusApiController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets a paginated list of SKUs (Products), optionally filtered by query matching Code or Name.
    /// </summary>
    /// <param name="query">Search term for SKU Code or SKU Name</param>
    /// <param name="page">Page number (1-based, default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 25, max: 100)</param>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<SkuDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<SkuDto>>>> GetAll(
        [FromQuery] string? query = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var skuQuery = _context.Skus
            .AsNoTracking()
            .Include(s => s.Uoms)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim().ToLower();
            skuQuery = skuQuery.Where(s => s.Code.ToLower().Contains(term) || s.Name.ToLower().Contains(term));
        }

        var totalCount = await skuQuery.CountAsync();

        var items = await skuQuery
            .OrderBy(s => s.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new SkuDto
            {
                Id = s.Id,
                UID = s.UID,
                Code = s.Code,
                Name = s.Name,
                Uoms = s.Uoms.Select(u => new UomOptionDto
                {
                    Code = u.Code,
                    Name = u.Name
                }).ToList()
            })
            .ToListAsync();

        var paged = new PagedResponse<SkuDto>
        {
            CurrentPage = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = items
        };

        return Ok(ApiResponse<PagedResponse<SkuDto>>.Ok(paged));
    }

    /// <summary>
    /// Gets a single SKU by its Code, including all associated UOMs.
    /// </summary>
    /// <param name="code">SKU Code (e.g. 3005406)</param>
    [HttpGet("{code}")]
    [ProducesResponseType(typeof(ApiResponse<SkuDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SkuDto>>> GetByCode(string code)
    {
        var sku = await _context.Skus
            .AsNoTracking()
            .Include(s => s.Uoms)
            .FirstOrDefaultAsync(s => s.Code == code);

        if (sku == null)
        {
            return NotFound(ApiResponse<SkuDto>.Fail($"SKU with Code '{code}' was not found."));
        }

        var dto = new SkuDto
        {
            Id = sku.Id,
            UID = sku.UID,
            Code = sku.Code,
            Name = sku.Name,
            Uoms = sku.Uoms.Select(u => new UomOptionDto
            {
                Code = u.Code,
                Name = u.Name
            }).ToList()
        };

        return Ok(ApiResponse<SkuDto>.Ok(dto));
    }

    /// <summary>
    /// Gets the list of available Units of Measure (UOMs) for a given SKU Code.
    /// </summary>
    /// <param name="code">SKU Code</param>
    [HttpGet("{code}/uoms")]
    [ProducesResponseType(typeof(ApiResponse<List<UomOptionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<List<UomOptionDto>>>> GetUomsForSku(string code)
    {
        var skuExists = await _context.Skus.AnyAsync(s => s.Code == code);
        if (!skuExists)
        {
            return NotFound(ApiResponse<List<UomOptionDto>>.Fail($"SKU with Code '{code}' was not found."));
        }

        var uoms = await _context.SkuUoms
            .AsNoTracking()
            .Where(u => u.SkuCode == code)
            .Select(u => new UomOptionDto
            {
                Code = u.Code,
                Name = u.Name
            })
            .ToListAsync();

        return Ok(ApiResponse<List<UomOptionDto>>.Ok(uoms));
    }

    /// <summary>
    /// Creates a new SKU with optional initial UOMs.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SkuDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SkuDto>>> Create([FromBody] CreateSkuRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<SkuDto>.Fail("Validation failed", errors));
        }

        var code = request.Code.Trim();
        if (await _context.Skus.AnyAsync(s => s.Code == code))
        {
            return BadRequest(ApiResponse<SkuDto>.Fail($"A SKU with Code '{code}' already exists."));
        }

        var sku = new Sku
        {
            UID = string.IsNullOrWhiteSpace(request.UID) ? "SKU-UID-" + Guid.NewGuid().ToString()[..8].ToUpper() : request.UID.Trim(),
            Code = code,
            Name = request.Name.Trim()
        };

        if (request.Uoms != null && request.Uoms.Count > 0)
        {
            foreach (var u in request.Uoms)
            {
                sku.Uoms.Add(new SkuUom
                {
                    UID = string.IsNullOrWhiteSpace(u.UID) ? Guid.NewGuid().ToString() : u.UID.Trim(),
                    SkuCode = sku.Code,
                    Code = u.Code.Trim(),
                    Name = u.Name.Trim()
                });
            }
        }
        else
        {
            // Default UOM: EA (Each)
            sku.Uoms.Add(new SkuUom
            {
                UID = Guid.NewGuid().ToString(),
                SkuCode = sku.Code,
                Code = "EA",
                Name = "Each"
            });
        }

        await _context.Skus.AddAsync(sku);
        await _context.SaveChangesAsync();

        var dto = new SkuDto
        {
            Id = sku.Id,
            UID = sku.UID,
            Code = sku.Code,
            Name = sku.Name,
            Uoms = sku.Uoms.Select(u => new UomOptionDto { Code = u.Code, Name = u.Name }).ToList()
        };

        return CreatedAtAction(nameof(GetByCode), new { code = sku.Code }, ApiResponse<SkuDto>.Ok(dto, "SKU created successfully."));
    }

    /// <summary>
    /// Deletes a SKU by its Code, provided it is not in use by any Load Template.
    /// </summary>
    /// <param name="code">SKU Code</param>
    [HttpDelete("{code}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(string code)
    {
        var sku = await _context.Skus.FirstOrDefaultAsync(s => s.Code == code);
        if (sku == null)
        {
            return NotFound(ApiResponse<object>.Fail($"SKU with Code '{code}' was not found."));
        }

        var isInUse = await _context.LoadTemplateLines.AnyAsync(l => l.SKUCode == code);
        if (isInUse)
        {
            return BadRequest(ApiResponse<object>.Fail($"Cannot delete SKU '{code}' because it is in use by one or more Load Templates."));
        }

        _context.Skus.Remove(sku);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { code, sku.Name }, $"SKU '{code}' deleted successfully."));
    }
}

