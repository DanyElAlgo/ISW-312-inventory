using Microsoft.EntityFrameworkCore;

namespace Purchases.API.Models;

public partial class PurchasesDbContext : DbContext
{
    public PurchasesDbContext()
    {
    }

    public PurchasesDbContext(DbContextOptions<PurchasesDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Business> Businesses { get; set; }
    public virtual DbSet<Supplier> Suppliers { get; set; }
    public virtual DbSet<PurchaseStatus> PurchaseStatuses { get; set; }
    public virtual DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    public virtual DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        const string schema = "purchases";

        modelBuilder.Entity<Business>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("business_pkey");
            entity.ToTable("business", "inventory");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasMaxLength(255).HasColumnName("name");
            entity.Property(e => e.Cen).HasMaxLength(64).HasColumnName("cen");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
        });

        modelBuilder.Entity<PurchaseStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("purchase_status_pkey");
            entity.ToTable("purchase_status", schema);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasMaxLength(255).HasColumnName("name");
            entity.Property(e => e.Description).HasMaxLength(255).HasColumnName("description");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("supplier_pkey");
            entity.ToTable("supplier", schema);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BusinessId).HasColumnName("business_id");
            entity.Property(e => e.Name).HasMaxLength(255).HasColumnName("name");
            entity.Property(e => e.Cen).HasMaxLength(64).HasColumnName("cen");
            entity.Property(e => e.ContactEmail).HasMaxLength(255).HasColumnName("contact_email");
            entity.Property(e => e.ContactPhone).HasMaxLength(50).HasColumnName("contact_phone");
            entity.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
        });

        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("purchase_order_pkey");
            entity.ToTable("purchase_order", schema);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BusinessId).HasColumnName("business_id");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.WarehouseCen).HasMaxLength(64).HasColumnName("warehouse_cen");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.Cen).HasMaxLength(64).HasColumnName("cen");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasDefaultValueSql("NOW()")
                .HasColumnName("created_at");
            entity.Property(e => e.ConfirmedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("confirmed_at");
            entity.Property(e => e.CancelledAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("cancelled_at");
            entity.Property(e => e.CancellationReason).HasMaxLength(500).HasColumnName("cancellation_reason");
            entity.Property(e => e.InventoryDocumentCen).HasMaxLength(64).HasColumnName("inventory_document_cen");

            entity.HasOne(d => d.Supplier).WithMany(p => p.PurchaseOrders)
                .HasForeignKey(d => d.SupplierId)
                .HasConstraintName("purchase_order_supplier_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.PurchaseOrders)
                .HasForeignKey(d => d.StatusId)
                .HasConstraintName("purchase_order_status_id_fkey");
        });

        modelBuilder.Entity<PurchaseOrderItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("purchase_order_item_pkey");
            entity.ToTable("purchase_order_item", schema);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PurchaseOrderId).HasColumnName("purchase_order_id");
            entity.Property(e => e.ProductCen).HasMaxLength(64).HasColumnName("product_cen");
            entity.Property(e => e.ProductName).HasMaxLength(255).HasColumnName("product_name");
            entity.Property(e => e.Quantity).HasColumnName("quantity");

            entity.HasOne(d => d.PurchaseOrder).WithMany(p => p.Items)
                .HasForeignKey(d => d.PurchaseOrderId)
                .HasConstraintName("purchase_order_item_purchase_order_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
