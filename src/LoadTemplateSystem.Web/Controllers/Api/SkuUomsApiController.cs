using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoadTemplateSystem.Web.Data;
using LoadTemplateSystem.Web.Models.Api;
using LoadTemplateSystem.Web.Models.Entities;

namespace LoadTemplateSystem.Web.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Route("api/skuuoms")]
[Produces("application/json")]
public class SkuUomsApiController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<SkuUomsApiController> _logger;

    public SkuUomsApiController(AppDbContext context, ILogger<SkuUomsApiController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets all SKU UOM mappings, optionally filtered by SKU Code.
    /// </summary>
    /// <param name="skuCode">Optional SKU Code filter</param>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<SkuUomDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<SkuUomDto>>>> GetAll([FromQuery] string? skuCode = null)
    {
        var query = _context.SkuUoms.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(skuCode))
        {
            var code = skuCode.Trim();
            query = query.Where(u => u.SkuCode == code);
        }

        var list = await query
            .OrderBy(u => u.SkuCode)
            .ThenBy(u => u.Code)
            .Select(u => new SkuUomDto
            {
                Id = u.Id,
                UID = u.UID,
                SkuCode = u.SkuCode,
                Code = u.Code,
                Name = u.Name
            })
            .ToListAsync();

        return Ok(ApiResponse<List<SkuUomDto>>.Ok(list));
    }

    /// <summary>
    /// Gets a single SKU UOM mapping by ID.
    /// </summary>
    /// <param name="id">SKUUOM ID</param>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SkuUomDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SkuUomDto>>> GetById(int id)
    {
        var uom = await _context.SkuUoms.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        if (uom == null)
        {
            return NotFound(ApiResponse<SkuUomDto>.Fail($"SKU UOM with ID #{id} was not found."));
        }

        var dto = new SkuUomDto
        {
            Id = uom.Id,
            UID = uom.UID,
            SkuCode = uom.SkuCode,
            Code = uom.Code,
            Name = uom.Name
        };

        return Ok(ApiResponse<SkuUomDto>.Ok(dto));
    }

    /// <summary>
    /// Adds a new UOM mapping for an existing SKU.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SkuUomDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SkuUomDto>>> Create([FromBody] CreateSkuUomMappingRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<SkuUomDto>.Fail("Validation failed", errors));
        }

        var skuCode = request.SkuCode.Trim();
        var skuExists = await _context.Skus.AnyAsync(s => s.Code == skuCode);
        if (!skuExists)
        {
            return BadRequest(ApiResponse<SkuUomDto>.Fail($"SKU with Code '{skuCode}' does not exist."));
        }

        var uomCode = request.Code.Trim();
        var alreadyExists = await _context.SkuUoms.AnyAsync(u => u.SkuCode == skuCode && u.Code == uomCode);
        if (alreadyExists)
        {
            return BadRequest(ApiResponse<SkuUomDto>.Fail($"UOM Code '{uomCode}' already exists for SKU '{skuCode}'."));
        }

        var entity = new SkuUom
        {
            UID = string.IsNullOrWhiteSpace(request.UID) ? Guid.NewGuid().ToString() : request.UID.Trim(),
            SkuCode = skuCode,
            Code = uomCode,
            Name = request.Name.Trim()
        };

        await _context.SkuUoms.AddAsync(entity);
        await _context.SaveChangesAsync();

        var dto = new SkuUomDto
        {
            Id = entity.Id,
            UID = entity.UID,
            SkuCode = entity.SkuCode,
            Code = entity.Code,
            Name = entity.Name
        };

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, ApiResponse<SkuUomDto>.Ok(dto, "SKU UOM created successfully."));
    }

    /// <summary>
    /// Deletes a SKU UOM mapping by ID.
    /// </summary>
    /// <param name="id">SKUUOM ID</param>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var uom = await _context.SkuUoms.FindAsync(id);
        if (uom == null)
        {
            return NotFound(ApiResponse<object>.Fail($"SKU UOM with ID #{id} was not found."));
        }

        // Safety check: is this UOM in use for this SKU?
        var isInUse = await _context.LoadTemplateLines.AnyAsync(l => l.SKUCode == uom.SkuCode && l.UOMCode == uom.Code);
        if (isInUse)
        {
            return BadRequest(ApiResponse<object>.Fail($"Cannot delete UOM '{uom.Code}' for SKU '{uom.SkuCode}' because it is in use by one or more Load Templates."));
        }

        _context.SkuUoms.Remove(uom);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { id, uom.SkuCode, uom.Code, uom.Name }, "SKU UOM mapping deleted successfully."));
    }
}

public class CreateSkuUomMappingRequest
{
    public string? UID { get; set; }

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "SKU Code is required")]
    [System.ComponentModel.DataAnnotations.StringLength(250)]
    public string SkuCode { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "UOM Code is required")]
    [System.ComponentModel.DataAnnotations.StringLength(250)]
    public string Code { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "UOM Name is required")]
    [System.ComponentModel.DataAnnotations.StringLength(250)]
    public string Name { get; set; } = string.Empty;
}

