using Microsoft.EntityFrameworkCore;
using LoadTemplateSystem.Web.Data;
using LoadTemplateSystem.Web.Models.Entities;
using LoadTemplateSystem.Web.Models.ViewModels;
using LoadTemplateSystem.Web.Services.Interfaces;

namespace LoadTemplateSystem.Web.Services;

public class LoadTemplateService : ILoadTemplateService
{
    private readonly AppDbContext _context;

    public LoadTemplateService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<LoadTemplateListVm> GetTemplatesPagedAsync(
        string? searchTerm,
        string? sortColumn,
        string? sortDirection,
        int pageNumber,
        int pageSize)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = pageSize <= 0 ? 10 : pageSize;
        sortColumn = string.IsNullOrWhiteSpace(sortColumn) ? "TemplateName" : sortColumn;
        sortDirection = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";

        var query = _context.LoadTemplates
            .Include(t => t.Lines)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.Trim().ToLower();
            query = query.Where(t =>
                t.UID.ToLower().Contains(search) ||
                t.TemplateName.ToLower().Contains(search) ||
                (t.TemplateDescription != null && t.TemplateDescription.ToLower().Contains(search)));
        }

        var isDesc = sortDirection == "desc";
        query = sortColumn switch
        {
            "UID" => isDesc ? query.OrderByDescending(t => t.UID) : query.OrderBy(t => t.UID),
            "TemplateName" => isDesc ? query.OrderByDescending(t => t.TemplateName) : query.OrderBy(t => t.TemplateName),
            "ItemCount" => isDesc
                ? query.OrderByDescending(t => t.Lines.Any()
                    ? t.Lines.Sum(l => l.MonQty + l.TueQty + l.WedQty + l.ThuQty + l.FriQty + l.SatQty + l.SunQty)
                    : (decimal)t.ItemCount)
                : query.OrderBy(t => t.Lines.Any()
                    ? t.Lines.Sum(l => l.MonQty + l.TueQty + l.WedQty + l.ThuQty + l.FriQty + l.SatQty + l.SunQty)
                    : (decimal)t.ItemCount),
            "TotalUnits" => isDesc
                ? query.OrderByDescending(t => t.Lines.Sum(l => l.MonQty + l.TueQty + l.WedQty + l.ThuQty + l.FriQty + l.SatQty + l.SunQty))
                : query.OrderBy(t => t.Lines.Sum(l => l.MonQty + l.TueQty + l.WedQty + l.ThuQty + l.FriQty + l.SatQty + l.SunQty)),
            _ => isDesc ? query.OrderByDescending(t => t.TemplateName) : query.OrderBy(t => t.TemplateName)
        };

        var totalItems = await query.CountAsync();

        var pagedItems = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new LoadTemplateSummaryVm
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

        return new LoadTemplateListVm
        {
            SearchTerm = searchTerm,
            SortColumn = sortColumn,
            SortDirection = sortDirection,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            Templates = pagedItems
        };
    }

    public async Task<LoadTemplateEditVm?> GetTemplateForEditAsync(int id)
    {
        var template = await _context.LoadTemplates
            .Include(t => t.Lines)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

        if (template == null) return null;

        return await BuildEditViewModelAsync(template);
    }

    public async Task<LoadTemplateEditVm?> GetTemplateForEditByUIDAsync(string uid)
    {
        var template = await _context.LoadTemplates
            .Include(t => t.Lines)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.UID == uid);

        if (template == null) return null;

        return await BuildEditViewModelAsync(template);
    }

    private async Task<LoadTemplateEditVm> BuildEditViewModelAsync(LoadTemplate template)
    {
        var skuCodes = template.Lines.Select(l => l.SKUCode).Distinct().ToList();

        // Get SKU Names map
        var skuNamesMap = await _context.Skus
            .Where(s => skuCodes.Contains(s.Code))
            .ToDictionaryAsync(s => s.Code, s => s.Name);

        // Get Available UOMs map (Code -> List<UomOptionVm>)
        var skuUomsMap = await _context.SkuUoms
            .Where(u => skuCodes.Contains(u.SkuCode))
            .GroupBy(u => u.SkuCode)
            .ToDictionaryAsync(
                g => g.Key,
                g => g.Select(u => new UomOptionVm { Code = u.Code, Name = u.Name }).ToList()
            );

        var vm = new LoadTemplateEditVm
        {
            Id = template.Id,
            UID = template.UID,
            TemplateName = template.TemplateName,
            TemplateDescription = template.TemplateDescription,
            Lines = template.Lines
                .Select((l, idx) =>
                {
                    skuNamesMap.TryGetValue(l.SKUCode, out var skuName);
                    skuUomsMap.TryGetValue(l.SKUCode, out var availableUoms);

                    if (availableUoms == null || availableUoms.Count == 0)
                    {
                        availableUoms = new List<UomOptionVm>
                        {
                            new UomOptionVm { Code = l.UOMCode, Name = l.UOMCode }
                        };
                    }

                    return new LoadTemplateLineVm
                    {
                        Id = l.Id,
                        UID = l.UID,
                        LoadTemplateUID = l.LoadTemplateUID,
                        SkuCode = l.SKUCode,
                        SkuName = skuName ?? l.SKUCode,
                        SelectedUomCode = l.UOMCode,
                        AvailableUoms = availableUoms,
                        MonQty = l.MonQty,
                        TueQty = l.TueQty,
                        WedQty = l.WedQty,
                        ThuQty = l.ThuQty,
                        FriQty = l.FriQty,
                        SatQty = l.SatQty,
                        SunQty = l.SunQty,
                        Sequence = idx + 1
                    };
                })
                .ToList()
        };

        return vm;
    }

    public async Task<int> SaveTemplateAsync(LoadTemplateEditVm model)
    {
        LoadTemplate entity;

        if (string.IsNullOrWhiteSpace(model.UID))
        {
            model.UID = Guid.NewGuid().ToString();
        }

        if (model.Id == 0)
        {
            entity = new LoadTemplate
            {
                UID = model.UID.Trim(),
                TemplateName = model.TemplateName.Trim(),
                TemplateDescription = model.TemplateDescription?.Trim()
            };
            await _context.LoadTemplates.AddAsync(entity);
            await _context.SaveChangesAsync();
        }
        else
        {
            var existing = await _context.LoadTemplates
                .Include(t => t.Lines)
                .FirstOrDefaultAsync(t => t.Id == model.Id);

            if (existing == null)
            {
                throw new InvalidOperationException($"Load Template with ID {model.Id} not found.");
            }

            entity = existing;
            if (!string.IsNullOrWhiteSpace(model.UID))
            {
                entity.UID = model.UID.Trim();
            }
            entity.TemplateName = model.TemplateName.Trim();
            entity.TemplateDescription = model.TemplateDescription?.Trim();
        }

        // Active lines submitted
        var activeLinesFromModel = model.Lines
            .Where(l => !l.IsDeleted && !string.IsNullOrWhiteSpace(l.SkuCode))
            .ToList();

        // Remove deleted lines
        var activeLineUids = activeLinesFromModel
            .Where(l => !string.IsNullOrWhiteSpace(l.UID))
            .Select(l => l.UID)
            .ToHashSet();

        var linesToRemove = entity.Lines.Where(l => !activeLineUids.Contains(l.UID)).ToList();
        foreach (var lineToRemove in linesToRemove)
        {
            _context.LoadTemplateLines.Remove(lineToRemove);
        }

        // Add or update active lines
        foreach (var lineModel in activeLinesFromModel)
        {
            var existingLine = entity.Lines.FirstOrDefault(l => l.UID == lineModel.UID || (lineModel.Id > 0 && l.Id == lineModel.Id));
            if (existingLine != null)
            {
                existingLine.UOMCode = lineModel.SelectedUomCode;
                existingLine.MonQty = Math.Max(0, lineModel.MonQty ?? 0);
                existingLine.TueQty = Math.Max(0, lineModel.TueQty ?? 0);
                existingLine.WedQty = Math.Max(0, lineModel.WedQty ?? 0);
                existingLine.ThuQty = Math.Max(0, lineModel.ThuQty ?? 0);
                existingLine.FriQty = Math.Max(0, lineModel.FriQty ?? 0);
                existingLine.SatQty = Math.Max(0, lineModel.SatQty ?? 0);
                existingLine.SunQty = Math.Max(0, lineModel.SunQty ?? 0);
            }
            else
            {
                entity.Lines.Add(new LoadTemplateLine
                {
                    UID = string.IsNullOrWhiteSpace(lineModel.UID) ? Guid.NewGuid().ToString() : lineModel.UID.Trim(),
                    LoadTemplateUID = entity.UID,
                    SKUCode = lineModel.SkuCode,
                    UOMCode = lineModel.SelectedUomCode,
                    MonQty = Math.Max(0, lineModel.MonQty ?? 0),
                    TueQty = Math.Max(0, lineModel.TueQty ?? 0),
                    WedQty = Math.Max(0, lineModel.WedQty ?? 0),
                    ThuQty = Math.Max(0, lineModel.ThuQty ?? 0),
                    FriQty = Math.Max(0, lineModel.FriQty ?? 0),
                    SatQty = Math.Max(0, lineModel.SatQty ?? 0),
                    SunQty = Math.Max(0, lineModel.SunQty ?? 0)
                });
            }
        }

        // Update ItemCount to store total weekly load quantity (sum of Mon-Sun across all items)
        var totalWeeklyUnits = activeLinesFromModel.Sum(l =>
            (l.MonQty ?? 0) + (l.TueQty ?? 0) + (l.WedQty ?? 0) +
            (l.ThuQty ?? 0) + (l.FriQty ?? 0) + (l.SatQty ?? 0) + (l.SunQty ?? 0));
        entity.ItemCount = (int)Math.Round(totalWeeklyUnits);

        await _context.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<bool> DeleteTemplateAsync(int id)
    {
        var template = await _context.LoadTemplates.FindAsync(id);
        if (template == null) return false;

        _context.LoadTemplates.Remove(template);
        await _context.SaveChangesAsync();
        return true;
    }
}
