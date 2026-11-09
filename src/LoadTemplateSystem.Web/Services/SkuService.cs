using Microsoft.EntityFrameworkCore;
using LoadTemplateSystem.Web.Data;
using LoadTemplateSystem.Web.Models.ViewModels;
using LoadTemplateSystem.Web.Services.Interfaces;

namespace LoadTemplateSystem.Web.Services;

public class SkuService : ISkuService
{
    private readonly AppDbContext _context;

    public SkuService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductSearchItemVm>> SearchProductsAsync(string? query, int maxResults = 25)
    {
        var dbQuery = _context.Skus
            .Include(s => s.Uoms)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var trimmed = query.Trim().ToLower();
            dbQuery = dbQuery.Where(s =>
                s.Code.ToLower().Contains(trimmed) ||
                s.Name.ToLower().Contains(trimmed));
        }

        var results = await dbQuery
            .OrderBy(s => s.Code)
            .Take(maxResults)
            .Select(s => new ProductSearchItemVm
            {
                Id = s.Id,
                UID = s.UID,
                Code = s.Code,
                Name = s.Name,
                DefaultUomCode = s.Uoms.Select(u => u.Code).FirstOrDefault() ?? "EA",
                AvailableUoms = s.Uoms.Select(u => new UomOptionVm
                {
                    Code = u.Code,
                    Name = u.Name
                }).ToList()
            })
            .ToListAsync();

        return results;
    }

    public async Task<List<UomOptionVm>> GetUomsForSkuCodeAsync(string skuCode)
    {
        var uoms = await _context.SkuUoms
            .Where(u => u.SkuCode == skuCode)
            .Select(u => new UomOptionVm
            {
                Code = u.Code,
                Name = u.Name
            })
            .ToListAsync();

        if (uoms.Count == 0)
        {
            uoms.Add(new UomOptionVm { Code = "EA", Name = "Each" });
        }

        return uoms;
    }

    public async Task<LoadTemplateLineVm?> CreateLineForSkuCodeAsync(string skuCode, string templateUid, int sequence = 0)
    {
        var sku = await _context.Skus
            .Include(s => s.Uoms)
            .FirstOrDefaultAsync(s => s.Code == skuCode);

        if (sku == null) return null;

        var uomList = sku.Uoms.Select(u => new UomOptionVm
        {
            Code = u.Code,
            Name = u.Name
        }).ToList();

        if (uomList.Count == 0)
        {
            uomList.Add(new UomOptionVm { Code = "EA", Name = "Each" });
        }

        var defaultUomCode = uomList.FirstOrDefault()?.Code ?? "EA";

        return new LoadTemplateLineVm
        {
            UID = Guid.NewGuid().ToString(),
            LoadTemplateUID = templateUid,
            SkuCode = sku.Code,
            SkuName = sku.Name,
            SelectedUomCode = defaultUomCode,
            AvailableUoms = uomList,
            MonQty = 0,
            TueQty = 0,
            WedQty = 0,
            ThuQty = 0,
            FriQty = 0,
            SatQty = 0,
            SunQty = 0,
            Sequence = sequence
        };
    }
}
