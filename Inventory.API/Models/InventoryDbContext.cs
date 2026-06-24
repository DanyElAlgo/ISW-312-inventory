using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Models;

public partial class InventoryDbContext : DbContext
{
    public InventoryDbContext()
    {
    }

    public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Business> Businesses { get; set; }
    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<InventoryDocument> InventoryDocuments { get; set; }
    public virtual DbSet<InventoryDocumentLine> InventoryDocumentLines { get; set; }
    public virtual DbSet<Kardex> Kardices { get; set; }
    public virtual DbSet<Product> Products { get; set; }
    public virtual DbSet<ProductStatus> ProductStatuses { get; set; }
    public virtual DbSet<Unit> Units { get; set; }
    public virtual DbSet<Warehouse> Warehouses { get; set; }
    public virtual DbSet<WarehouseProduct> WarehouseProducts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Only fall back to Npgsql when the context was not already configured by the
        // caller (e.g. AddDbContext in Program.cs, or the InMemory provider in tests).
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseNpgsql("Name=ConnectionStrings:DefaultConnection");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        const string schema = "inventory";

        modelBuilder.Entity<Business>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("business_pkey");
            entity.ToTable("business", schema);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Cen)
                .HasMaxLength(64)
                .HasColumnName("cen");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("category_pkey");
            entity.ToTable("category", schema);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BusinessId).HasColumnName("business_id");
            entity.Property(e => e.Cen)
                .HasMaxLength(64)
                .HasColumnName("cen");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");

            entity.HasOne(d => d.Business).WithMany()
                .HasForeignKey(d => d.BusinessId)
                .HasConstraintName("category_business_id_fkey");
        });

        modelBuilder.Entity<Kardex>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("kardex_pkey");
            entity.ToTable("kardex", schema);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActionQty).HasColumnName("action_qty");
            entity.Property(e => e.ActionType)
                .HasMaxLength(50)
                .HasColumnName("action_type");
            entity.Property(e => e.DocumentId).HasColumnName("document_id");
            entity.Property(e => e.MovementCen)
                .HasMaxLength(64)
                .HasColumnName("movement_cen");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Reason)
                .HasMaxLength(255)
                .HasColumnName("reason");
            entity.Property(e => e.TimeStamp)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("time_stamp");
            entity.Property(e => e.WarehouseId).HasColumnName("warehouse_id");

            entity.HasOne(d => d.Product).WithMany(p => p.Kardices)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("kardex_product_id_fkey");

            entity.HasOne(d => d.Document).WithMany(p => p.KardexEntries)
                .HasForeignKey(d => d.DocumentId)
                .HasConstraintName("kardex_document_id_fkey");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.Kardices)
                .HasForeignKey(d => d.WarehouseId)
                .HasConstraintName("kardex_warehouse_id_fkey");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("product_pkey");
            entity.ToTable("product", schema);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BusinessId).HasColumnName("business_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Cen)
                .HasMaxLength(64)
                .HasColumnName("cen");
            entity.Property(e => e.Sku)
                .HasMaxLength(100)
                .HasColumnName("sku");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Price)
                .HasColumnType("numeric(12,2)")
                .HasColumnName("price");
            entity.Property(e => e.CostPrice)
                .HasColumnType("numeric(12,2)")
                .HasColumnName("cost_price");
            entity.Property(e => e.ReorderLevel)
                .HasDefaultValue(0)
                .HasColumnName("reorder_level");
            entity.Property(e => e.StationCode)
                .HasMaxLength(50)
                .HasColumnName("station_code");
            entity.Property(e => e.UnitId).HasColumnName("unit_id");
            entity.Property(e => e.UnitQty).HasColumnName("unit_qty");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("product_category_id_fkey");

            entity.HasOne(d => d.Business).WithMany()
                .HasForeignKey(d => d.BusinessId)
                .HasConstraintName("product_business_id_fkey");

            entity.HasOne(d => d.Unit).WithMany(p => p.Products)
                .HasForeignKey(d => d.UnitId)
                .HasConstraintName("product_unit_id_fkey");
        });

        modelBuilder.Entity<ProductStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("product_status_pkey");
            entity.ToTable("product_status", schema);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Unit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("unit_pkey");
            entity.ToTable("unit", schema);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Abbreviation)
                .HasMaxLength(50)
                .HasColumnName("abbreviation");
            entity.Property(e => e.BusinessId).HasColumnName("business_id");
            entity.Property(e => e.Cen)
                .HasMaxLength(64)
                .HasColumnName("cen");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");

            entity.HasOne(d => d.Business).WithMany()
                .HasForeignKey(d => d.BusinessId)
                .HasConstraintName("unit_business_id_fkey");
        });

        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("warehouse_pkey");
            entity.ToTable("warehouse", schema);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BusinessId).HasColumnName("business_id");
            entity.Property(e => e.Cen)
                .HasMaxLength(64)
                .HasColumnName("cen");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");

            entity.HasOne(d => d.Business).WithMany(p => p.Warehouses)
                .HasForeignKey(d => d.BusinessId)
                .HasConstraintName("warehouse_business_id_fkey");
        });

        modelBuilder.Entity<WarehouseProduct>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("warehouse_product_pkey");
            entity.ToTable("warehouse_product", schema);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.LowStockQty).HasColumnName("low_stock_qty");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.StockLeft).HasColumnName("stock_left");
            entity.Property(e => e.WarehouseId).HasColumnName("warehouse_id");

            entity.HasOne(d => d.Product).WithMany(p => p.WarehouseProducts)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("warehouse_product_product_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.WarehouseProducts)
                .HasForeignKey(d => d.StatusId)
                .HasConstraintName("warehouse_product_status_id_fkey");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.WarehouseProducts)
                .HasForeignKey(d => d.WarehouseId)
                .HasConstraintName("warehouse_product_warehouse_id_fkey");
        });

        modelBuilder.Entity<InventoryDocument>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("inventory_document_pkey");
            entity.ToTable("inventory_document", schema);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BusinessId).HasColumnName("business_id");
            entity.Property(e => e.WarehouseId).HasColumnName("warehouse_id");
            entity.Property(e => e.DocumentCen)
                .HasMaxLength(64)
                .HasColumnName("document_cen");
            entity.Property(e => e.DocumentType)
                .HasMaxLength(50)
                .HasColumnName("document_type");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.Reason)
                .HasMaxLength(255)
                .HasColumnName("reason");
            entity.Property(e => e.ExternalReference)
                .HasMaxLength(255)
                .HasColumnName("external_reference");
            entity.Property(e => e.Source)
                .HasMaxLength(50)
                .HasColumnName("source");
            entity.Property(e => e.ReferenceCen)
                .HasMaxLength(100)
                .HasColumnName("reference_cen");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");

            entity.HasOne(d => d.Business).WithMany()
                .HasForeignKey(d => d.BusinessId)
                .HasConstraintName("inventory_document_business_id_fkey");

            entity.HasOne(d => d.Warehouse).WithMany()
                .HasForeignKey(d => d.WarehouseId)
                .HasConstraintName("inventory_document_warehouse_id_fkey");
        });

        modelBuilder.Entity<InventoryDocumentLine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("inventory_document_line_pkey");
            entity.ToTable("inventory_document_line", schema);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DocumentId).HasColumnName("document_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.UnitCost)
                .HasColumnType("numeric(12,2)")
                .HasColumnName("unit_cost");

            entity.HasOne(d => d.Document).WithMany(p => p.Lines)
                .HasForeignKey(d => d.DocumentId)
                .HasConstraintName("inventory_document_line_document_id_fkey");

            entity.HasOne(d => d.Product).WithMany()
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("inventory_document_line_product_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
