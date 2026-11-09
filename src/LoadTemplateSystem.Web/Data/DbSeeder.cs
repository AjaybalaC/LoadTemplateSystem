using Microsoft.EntityFrameworkCore;
using LoadTemplateSystem.Web.Models.Entities;

namespace LoadTemplateSystem.Web.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (await context.LoadTemplates.CountAsync() >= 20)
        {
            return; // Already seeded with 20 dummy records
        }

        // Clean existing tables to ensure clean 20 test records
        context.LoadTemplateLines.RemoveRange(context.LoadTemplateLines);
        context.LoadTemplates.RemoveRange(context.LoadTemplates);
        context.SkuUoms.RemoveRange(context.SkuUoms);
        context.Skus.RemoveRange(context.Skus);
        await context.SaveChangesAsync();

        // =========================================================================
        // 20 SKUs (Table: SKU & Table: SKUUOM)
        // =========================================================================
        var skus = new List<Sku>
        {
            // Dairy / Milk (From PDF)
            new Sku
            {
                UID = "SKU-UID-3005406",
                Code = "3005406",
                Name = "Anc Fresh Dairy white Milk Blue 3L BOT",
                Uoms = new List<SkuUom>
                {
                    new SkuUom { UID = "UOM-3005406-OU", SkuCode = "3005406", Code = "OU", Name = "Outer" },
                    new SkuUom { UID = "UOM-3005406-EA", SkuCode = "3005406", Code = "EA", Name = "Each" },
                    new SkuUom { UID = "UOM-3005406-CT", SkuCode = "3005406", Code = "CT", Name = "Carton" }
                }
            },
            new Sku
            {
                UID = "SKU-UID-3005405",
                Code = "3005405",
                Name = "Anc Fresh Dairy white Milk Blue 2L BOT",
                Uoms = new List<SkuUom>
                {
                    new SkuUom { UID = "UOM-3005405-OU", SkuCode = "3005405", Code = "OU", Name = "Outer" },
                    new SkuUom { UID = "UOM-3005405-EA", SkuCode = "3005405", Code = "EA", Name = "Each" },
                    new SkuUom { UID = "UOM-3005405-CT", SkuCode = "3005405", Code = "CT", Name = "Carton" }
                }
            },
            new Sku
            {
                UID = "SKU-UID-545252",
                Code = "545252",
                Name = "Anc Fresh Dairy White Milk Blue 3L BOT",
                Uoms = new List<SkuUom>
                {
                    new SkuUom { UID = "UOM-545252-EA", SkuCode = "545252", Code = "EA", Name = "Each" },
                    new SkuUom { UID = "UOM-545252-OU", SkuCode = "545252", Code = "OU", Name = "Outer" },
                    new SkuUom { UID = "UOM-545252-CT", SkuCode = "545252", Code = "CT", Name = "Carton" }
                }
            },
            new Sku
            {
                UID = "SKU-UID-458692",
                Code = "458692",
                Name = "Anc Fresh Dairy White Milk Lite 3L BOT",
                Uoms = new List<SkuUom>
                {
                    new SkuUom { UID = "UOM-458692-EA", SkuCode = "458692", Code = "EA", Name = "Each" },
                    new SkuUom { UID = "UOM-458692-OU", SkuCode = "458692", Code = "OU", Name = "Outer" },
                    new SkuUom { UID = "UOM-458692-CT", SkuCode = "458692", Code = "CT", Name = "Carton" }
                }
            },
            new Sku
            {
                UID = "SKU-UID-668458",
                Code = "668458",
                Name = "Anc Fresh Dairy White Milk Silver 3L BOT",
                Uoms = new List<SkuUom>
                {
                    new SkuUom { UID = "UOM-668458-EA", SkuCode = "668458", Code = "EA", Name = "Each" },
                    new SkuUom { UID = "UOM-668458-OU", SkuCode = "668458", Code = "OU", Name = "Outer" },
                    new SkuUom { UID = "UOM-668458-CT", SkuCode = "668458", Code = "CT", Name = "Carton" }
                }
            },
            new Sku
            {
                UID = "SKU-UID-774554",
                Code = "774554",
                Name = "Anc Fresh Dairy White Milk Trim 3L BOT",
                Uoms = new List<SkuUom>
                {
                    new SkuUom { UID = "UOM-774554-EA", SkuCode = "774554", Code = "EA", Name = "Each" },
                    new SkuUom { UID = "UOM-774554-OU", SkuCode = "774554", Code = "OU", Name = "Outer" },
                    new SkuUom { UID = "UOM-774554-CT", SkuCode = "774554", Code = "CT", Name = "Carton" }
                }
            },
            new Sku
            {
                UID = "SKU-UID-564554",
                Code = "564554",
                Name = "Anc Fresh Dairy White Milk Calc+2L BOT",
                Uoms = new List<SkuUom>
                {
                    new SkuUom { UID = "UOM-564554-EA", SkuCode = "564554", Code = "EA", Name = "Each" },
                    new SkuUom { UID = "UOM-564554-OU", SkuCode = "564554", Code = "OU", Name = "Outer" },
                    new SkuUom { UID = "UOM-564554-CT", SkuCode = "564554", Code = "CT", Name = "Carton" }
                }
            },
            // Bakery / Bread
            new Sku
            {
                UID = "SKU-UID-100201",
                Code = "100201",
                Name = "Tip Top Super Soft White Bread 700g",
                Uoms = new List<SkuUom>
                {
                    new SkuUom { UID = "UOM-100201-EA", SkuCode = "100201", Code = "EA", Name = "Each" },
                    new SkuUom { UID = "UOM-100201-OU", SkuCode = "100201", Code = "OU", Name = "Outer" },
                    new SkuUom { UID = "UOM-100201-CT", SkuCode = "100201", Code = "CT", Name = "Carton" }
                }
            },
            new Sku
            {
                UID = "SKU-UID-100202",
                Code = "100202",
                Name = "Tip Top Wholemeal Sliced Bread 700g",
                Uoms = new List<SkuUom>
                {
                    new SkuUom { UID = "UOM-100202-EA", SkuCode = "100202", Code = "EA", Name = "Each" },
                    new SkuUom { UID = "UOM-100202-OU", SkuCode = "100202", Code = "OU", Name = "Outer" },
                    new SkuUom { UID = "UOM-100202-CT", SkuCode = "100202", Code = "CT", Name = "Carton" }
                }
            },
            new Sku
            {
                UID = "SKU-UID-100203",
                Code = "100203",
                Name = "Quality Baker English Muffins 6pk",
                Uoms = new List<SkuUom>
                {
                    new SkuUom { UID = "UOM-100203-EA", SkuCode = "100203", Code = "EA", Name = "Each" },
                    new SkuUom { UID = "UOM-100203-OU", SkuCode = "100203", Code = "OU", Name = "Outer" },
                    new SkuUom { UID = "UOM-100203-CT", SkuCode = "100203", Code = "CT", Name = "Carton" }
                }
            },
            // Cheese & Butter
            new Sku
            {
                UID = "SKU-UID-200301",
                Code = "200301",
                Name = "Mainland Tasty Cheddar Cheese Block 500g",
                Uoms = new List<SkuUom>
                {
                    new SkuUom { UID = "UOM-200301-EA", SkuCode = "200301", Code = "EA", Name = "Each" },
                    new SkuUom { UID = "UOM-200301-OU", SkuCode = "200301", Code = "OU", Name = "Outer" },
                    new SkuUom { UID = "UOM-200301-CT", SkuCode = "200301", Code = "CT", Name = "Carton" }
                }
            },
            new Sku
            {
                UID = "SKU-UID-200302",
                Code = "200302",
                Name = "Mainland Mild Cheddar Slices 250g",
                Uoms = new List<SkuUom>
                {
                    new SkuUom { UID = "UOM-200302-EA", SkuCode = "200302", Code = "EA", Name = "Each" },
                    new SkuUom { UID = "UOM-200302-OU", SkuCode = "200302", Code = "OU", Name = "Outer" },
                    new SkuUom { UID = "UOM-200302-CT", SkuCode = "200302", Code = "CT", Name = "Carton" }
                }
            },
            new Sku
            {
                UID = "SKU-UID-200303",
                Code = "200303",
                Name = "Anchor Salted Pure Creamery Butter 500g",
                Uoms = new List<SkuUom>
                {
                    new SkuUom { UID = "UOM-200303-EA", SkuCode = "200303", Code = "EA", Name = "Each" },
                    new SkuUom { UID = "UOM-200303-OU", SkuCode = "200303", Code = "OU", Name = "Outer" },
                    new SkuUom { UID = "UOM-200303-CT", SkuCode = "200303", Code = "CT", Name = "Carton" }
                }
            },
            // Produce / Fresh Fruits & Veg
            new Sku
            {
                UID = "SKU-UID-300401",
                Code = "300401",
                Name = "Fresh Crisp Gala Apples 1.5kg Bag",
                Uoms = new List<SkuUom>
                {
                    new SkuUom { UID = "UOM-300401-EA", SkuCode = "300401", Code = "EA", Name = "Each" },
                    new SkuUom { UID = "UOM-300401-OU", SkuCode = "300401", Code = "OU", Name = "Outer" },
                    new SkuUom { UID = "UOM-300401-CT", SkuCode = "300401", Code = "CT", Name = "Carton" }
                }
            },
            new Sku
            {
                UID = "SKU-UID-300402",
                Code = "300402",
                Name = "Fresh Cavendish Bananas Cluster 1kg",
                Uoms = new List<SkuUom>
                {
                    new SkuUom { UID = "UOM-300402-EA", SkuCode = "300402", Code = "EA", Name = "Each" },
                    new SkuUom { UID = "UOM-300402-OU", SkuCode = "300402", Code = "OU", Name = "Outer" },
                    new SkuUom { UID = "UOM-300402-CT", SkuCode = "300402", Code = "CT", Name = "Carton" }
                }
            },
            new Sku
            {
                UID = "SKU-UID-300403",
                Code = "300403",
                Name = "Washed Russet Potatoes Prepack 2kg",
                Uoms = new List<SkuUom>
                {
                    new SkuUom { UID = "UOM-300403-EA", SkuCode = "300403", Code = "EA", Name = "Each" },
                    new SkuUom { UID = "UOM-300403-OU", SkuCode = "300403", Code = "OU", Name = "Outer" },
                    new SkuUom { UID = "UOM-300403-CT", SkuCode = "300403", Code = "CT", Name = "Carton" }
                }
            },
            // Beverages
            new Sku
            {
                UID = "SKU-UID-400501",
                Code = "400501",
                Name = "Coca-Cola Classic Can 330ml 6pk",
                Uoms = new List<SkuUom>
                {
                    new SkuUom { UID = "UOM-400501-EA", SkuCode = "400501", Code = "EA", Name = "Each" },
                    new SkuUom { UID = "UOM-400501-OU", SkuCode = "400501", Code = "OU", Name = "Outer" },
                    new SkuUom { UID = "UOM-400501-CT", SkuCode = "400501", Code = "CT", Name = "Carton" }
                }
            },
            new Sku
            {
                UID = "SKU-UID-400502",
                Code = "400502",
                Name = "Pump Pure Spring Water Bottle 750ml",
                Uoms = new List<SkuUom>
                {
                    new SkuUom { UID = "UOM-400502-EA", SkuCode = "400502", Code = "EA", Name = "Each" },
                    new SkuUom { UID = "UOM-400502-OU", SkuCode = "400502", Code = "OU", Name = "Outer" },
                    new SkuUom { UID = "UOM-400502-CT", SkuCode = "400502", Code = "CT", Name = "Carton" }
                }
            },
            new Sku
            {
                UID = "SKU-UID-400503",
                Code = "400503",
                Name = "Keri Premium Orange Juice 1L",
                Uoms = new List<SkuUom>
                {
                    new SkuUom { UID = "UOM-400503-EA", SkuCode = "400503", Code = "EA", Name = "Each" },
                    new SkuUom { UID = "UOM-400503-OU", SkuCode = "400503", Code = "OU", Name = "Outer" },
                    new SkuUom { UID = "UOM-400503-CT", SkuCode = "400503", Code = "CT", Name = "Carton" }
                }
            },
            new Sku
            {
                UID = "SKU-UID-500601",
                Code = "500601",
                Name = "Bluebird Original Salted Potato Chips 150g",
                Uoms = new List<SkuUom>
                {
                    new SkuUom { UID = "UOM-500601-EA", SkuCode = "500601", Code = "EA", Name = "Each" },
                    new SkuUom { UID = "UOM-500601-OU", SkuCode = "500601", Code = "OU", Name = "Outer" },
                    new SkuUom { UID = "UOM-500601-CT", SkuCode = "500601", Code = "CT", Name = "Carton" }
                }
            }
        };

        await context.Skus.AddRangeAsync(skus);
        await context.SaveChangesAsync();

        // =========================================================================
        // 20 Load Templates (Table: Load Template & Table: LoadTemplateLine)
        // =========================================================================
        var templates = new List<LoadTemplate>
        {
            // 01. From PDF Wireframe
            new LoadTemplate
            {
                UID = "LT-UID-001",
                TemplateName = "KL0021_CY0000720_LT",
                TemplateDescription = "TE AWAMUTU WEST",
                ItemCount = 39,
                Lines = new List<LoadTemplateLine>
                {
                    new LoadTemplateLine
                    {
                        UID = "LTL-UID-001",
                        LoadTemplateUID = "LT-UID-001",
                        SKUCode = "3005406",
                        UOMCode = "OU",
                        MonQty = 10,
                        TueQty = 5,
                        WedQty = 5,
                        ThuQty = 5,
                        FriQty = 10,
                        SatQty = 0,
                        SunQty = 0
                    },
                    new LoadTemplateLine
                    {
                        UID = "LTL-UID-002",
                        LoadTemplateUID = "LT-UID-001",
                        SKUCode = "3005405",
                        UOMCode = "OU",
                        MonQty = 2,
                        TueQty = 2,
                        WedQty = 0,
                        ThuQty = 0,
                        FriQty = 0,
                        SatQty = 0,
                        SunQty = 0
                    }
                }
            },
            // 02. From PDF Wireframe
            new LoadTemplate
            {
                UID = "LT-UID-002",
                TemplateName = "KL0021_CY0000724_LT",
                TemplateDescription = "TE KUITIE",
                ItemCount = 31,
                Lines = new List<LoadTemplateLine>
                {
                    new LoadTemplateLine
                    {
                        UID = "LTL-UID-003",
                        LoadTemplateUID = "LT-UID-002",
                        SKUCode = "3005406",
                        UOMCode = "EA",
                        MonQty = 10,
                        TueQty = 5,
                        WedQty = 5,
                        ThuQty = 5,
                        FriQty = 5,
                        SatQty = 0,
                        SunQty = 1
                    }
                }
            },
            // 03. From PDF Wireframe (NA)
            new LoadTemplate
            {
                UID = "LT-UID-003",
                TemplateName = "130021_CY0000729_LT",
                TemplateDescription = "QUICKPOST2",
                ItemCount = 0 // Displays as NA
            },
            // 04. From PDF Wireframe
            new LoadTemplate
            {
                UID = "LT-UID-004",
                TemplateName = "130021_R500720_LT",
                TemplateDescription = "MARKET MART",
                ItemCount = 12,
                Lines = new List<LoadTemplateLine>
                {
                    new LoadTemplateLine
                    {
                        UID = "LTL-UID-004",
                        LoadTemplateUID = "LT-UID-004",
                        SKUCode = "545252",
                        UOMCode = "EA",
                        MonQty = 2,
                        TueQty = 2,
                        WedQty = 2,
                        ThuQty = 2,
                        FriQty = 2,
                        SatQty = 1,
                        SunQty = 1
                    }
                }
            },
            // 05. From PDF Wireframe
            new LoadTemplate
            {
                UID = "LT-UID-005",
                TemplateName = "130021_R500721_LT",
                TemplateDescription = "SMALL",
                ItemCount = 24,
                Lines = new List<LoadTemplateLine>
                {
                    new LoadTemplateLine
                    {
                        UID = "LTL-UID-005",
                        LoadTemplateUID = "LT-UID-005",
                        SKUCode = "458692",
                        UOMCode = "EA",
                        MonQty = 4,
                        TueQty = 4,
                        WedQty = 4,
                        ThuQty = 4,
                        FriQty = 4,
                        SatQty = 2,
                        SunQty = 2
                    }
                }
            },
            // 06. Additional test templates
            new LoadTemplate
            {
                UID = "LT-UID-006",
                TemplateName = "130021_R500725_LT",
                TemplateDescription = "HAMILTON CENTRAL SUPERSTORE",
                ItemCount = 18
            },
            // 07
            new LoadTemplate
            {
                UID = "LT-UID-007",
                TemplateName = "KL0022_CY0000810_LT",
                TemplateDescription = "AUCKLAND CBD EXPRESS",
                ItemCount = 45
            },
            // 08
            new LoadTemplate
            {
                UID = "LT-UID-008",
                TemplateName = "KL0022_CY0000815_LT",
                TemplateDescription = "WELLINGTON HARBOUR RESTOCK",
                ItemCount = 22
            },
            // 09 (N/A)
            new LoadTemplate
            {
                UID = "LT-UID-009",
                TemplateName = "130022_CY0000830_LT",
                TemplateDescription = "CHRISTCHURCH METRO HUB",
                ItemCount = 0 // Displays as N/A
            },
            // 10
            new LoadTemplate
            {
                UID = "LT-UID-010",
                TemplateName = "130022_R500735_LT",
                TemplateDescription = "DUNEDIN SOUTH CONVENIENCE",
                ItemCount = 15
            },
            // 11
            new LoadTemplate
            {
                UID = "LT-UID-011",
                TemplateName = "KL0023_CY0000901_LT",
                TemplateDescription = "TAURANGA CROSSING MALL",
                ItemCount = 28
            },
            // 12 (N/A)
            new LoadTemplate
            {
                UID = "LT-UID-012",
                TemplateName = "KL0023_CY0000905_LT",
                TemplateDescription = "ROTORUA CENTRAL DEPOT",
                ItemCount = 0 // Displays as N/A
            },
            // 13
            new LoadTemplate
            {
                UID = "LT-UID-013",
                TemplateName = "130023_CY0000910_LT",
                TemplateDescription = "NAPIER COASTAL DISPATCH",
                ItemCount = 14
            },
            // 14
            new LoadTemplate
            {
                UID = "LT-UID-014",
                TemplateName = "130023_R500742_LT",
                TemplateDescription = "PALMERSTON NORTH LOGISTICS",
                ItemCount = 30
            },
            // 15
            new LoadTemplate
            {
                UID = "LT-UID-015",
                TemplateName = "KL0024_CY0000920_LT",
                TemplateDescription = "NELSON SUNSHINE GROCERS",
                ItemCount = 19
            },
            // 16 (N/A)
            new LoadTemplate
            {
                UID = "LT-UID-016",
                TemplateName = "KL0024_CY0000925_LT",
                TemplateDescription = "INVERCARGILL MART",
                ItemCount = 0 // Displays as N/A
            },
            // 17
            new LoadTemplate
            {
                UID = "LT-UID-017",
                TemplateName = "130024_CY0000930_LT",
                TemplateDescription = "QUEENSTOWN RESORT SUPPLY",
                ItemCount = 26
            },
            // 18
            new LoadTemplate
            {
                UID = "LT-UID-018",
                TemplateName = "130024_R500755_LT",
                TemplateDescription = "WHANGAREI NORTH MARKET",
                ItemCount = 16
            },
            // 19
            new LoadTemplate
            {
                UID = "LT-UID-019",
                TemplateName = "KL0025_CY0000940_LT",
                TemplateDescription = "NEW PLYMOUTH WEST DEPOT",
                ItemCount = 33
            },
            // 20
            new LoadTemplate
            {
                UID = "LT-UID-020",
                TemplateName = "KL0025_CY0000945_LT",
                TemplateDescription = "GISBORNE EAST FRESH MARKET",
                ItemCount = 21
            }
        };

        await context.LoadTemplates.AddRangeAsync(templates);
        await context.SaveChangesAsync();
    }
}
