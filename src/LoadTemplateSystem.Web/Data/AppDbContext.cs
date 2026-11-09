using Microsoft.EntityFrameworkCore;
using LoadTemplateSystem.Web.Models.Entities;

namespace LoadTemplateSystem.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<LoadTemplate> LoadTemplates => Set<LoadTemplate>();
    public DbSet<LoadTemplateLine> LoadTemplateLines => Set<LoadTemplateLine>();
    public DbSet<Sku> Skus => Set<Sku>();
    public DbSet<SkuUom> SkuUoms => Set<SkuUom>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Table: Load Template
        modelBuilder.Entity<LoadTemplate>(entity =>
        {
            entity.ToTable("LoadTemplate");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.UID).HasMaxLength(250).IsRequired();
            entity.Property(e => e.TemplateName).HasMaxLength(250).IsRequired();
            entity.Property(e => e.TemplateDescription).HasMaxLength(250);
            entity.Property(e => e.ItemCount);

            entity.HasIndex(e => e.UID).IsUnique();

            entity.HasMany(e => e.Lines)
                  .WithOne(l => l.LoadTemplate)
                  .HasPrincipalKey(e => e.UID)
                  .HasForeignKey(l => l.LoadTemplateUID)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Table: LoadTemplateLine
        modelBuilder.Entity<LoadTemplateLine>(entity =>
        {
            entity.ToTable("LoadTemplateLine");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.UID).HasMaxLength(250).IsRequired();
            entity.Property(e => e.LoadTemplateUID).HasMaxLength(250).IsRequired();
            entity.Property(e => e.SKUCode).HasMaxLength(250).IsRequired();
            entity.Property(e => e.UOMCode).HasMaxLength(250).IsRequired();

            entity.Property(e => e.MonQty).HasPrecision(18, 2);
            entity.Property(e => e.TueQty).HasPrecision(18, 2);
            entity.Property(e => e.WedQty).HasPrecision(18, 2);
            entity.Property(e => e.ThuQty).HasPrecision(18, 2);
            entity.Property(e => e.FriQty).HasPrecision(18, 2);
            entity.Property(e => e.SatQty).HasPrecision(18, 2);
            entity.Property(e => e.SunQty).HasPrecision(18, 2);

            entity.HasIndex(e => e.UID).IsUnique();
            entity.HasIndex(e => e.LoadTemplateUID);
        });

        // Table: SKU
        modelBuilder.Entity<Sku>(entity =>
        {
            entity.ToTable("SKU");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.UID).HasMaxLength(250).IsRequired();
            entity.Property(e => e.Code).HasMaxLength(250).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(250).IsRequired();

            entity.HasIndex(e => e.UID).IsUnique();
            entity.HasIndex(e => e.Code).IsUnique();

            entity.HasMany(e => e.Uoms)
                  .WithOne(u => u.Sku)
                  .HasPrincipalKey(s => s.Code)
                  .HasForeignKey(u => u.SkuCode)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Table: SKUUOM
        modelBuilder.Entity<SkuUom>(entity =>
        {
            entity.ToTable("SKUUOM");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.UID).HasMaxLength(250).IsRequired();
            entity.Property(e => e.SkuCode).HasColumnName("SKU Code").HasMaxLength(250).IsRequired();
            entity.Property(e => e.Code).HasMaxLength(250).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(250).IsRequired();

            entity.HasIndex(e => e.UID).IsUnique();
            entity.HasIndex(e => e.SkuCode);
        });
    }
}
