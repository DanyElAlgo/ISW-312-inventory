using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace InventoryAPI.Models;

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

    public virtual DbSet<Kardex> Kardices { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Warehouse> Warehouses { get; set; }

    public virtual DbSet<Warehouseproduct> Warehouseproducts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Business>(entity =>
        {
            entity.HasKey(e => e.Businessid).HasName("business_pkey");

            entity.ToTable("business");

            entity.HasIndex(e => e.Businessname, "business_businessname_key").IsUnique();

            entity.Property(e => e.Businessid)
                .UseIdentityAlwaysColumn()
                .HasColumnName("businessid");
            entity.Property(e => e.Businessname)
                .HasMaxLength(50)
                .HasColumnName("businessname");
        });

        modelBuilder.Entity<Kardex>(entity =>
        {
            entity.HasKey(e => e.Kardexid).HasName("kardex_pkey");

            entity.ToTable("kardex");

            entity.Property(e => e.Kardexid)
                .UseIdentityAlwaysColumn()
                .HasColumnName("kardexid");
            entity.Property(e => e.Actionqty).HasColumnName("actionqty");
            entity.Property(e => e.Actiontype)
                .HasMaxLength(10)
                .HasColumnName("actiontype");
            entity.Property(e => e.Businessid).HasColumnName("businessid");
            entity.Property(e => e.Productid).HasColumnName("productid");
            entity.Property(e => e.Reason)
                .HasMaxLength(100)
                .HasColumnName("reason");
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("timestamp");
            entity.Property(e => e.Warehouseprimaryid).HasColumnName("warehouseprimaryid");
            entity.Property(e => e.Warehousesecondaryid).HasColumnName("warehousesecondaryid");

            entity.HasOne(d => d.Business).WithMany(p => p.Kardices)
                .HasForeignKey(d => d.Businessid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_kardex_business");

            entity.HasOne(d => d.Product).WithMany(p => p.Kardices)
                .HasForeignKey(d => d.Productid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_kardex_product");

            entity.HasOne(d => d.Warehouseprimary).WithMany(p => p.KardexWarehouseprimaries)
                .HasForeignKey(d => d.Warehouseprimaryid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_wh1");

            entity.HasOne(d => d.Warehousesecondary).WithMany(p => p.KardexWarehousesecondaries)
                .HasForeignKey(d => d.Warehousesecondaryid)
                .HasConstraintName("fk_wh2");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Productid).HasName("product_pkey");

            entity.ToTable("product");

            entity.Property(e => e.Productid)
                .UseIdentityAlwaysColumn()
                .HasColumnName("productid");
            entity.Property(e => e.Batch).HasColumnName("batch");
            entity.Property(e => e.Category)
                .HasMaxLength(20)
                .HasColumnName("category");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("description");
            entity.Property(e => e.Lowstockqty).HasColumnName("lowstockqty");
            entity.Property(e => e.Metricunit)
                .HasMaxLength(20)
                .HasColumnName("metricunit");
            entity.Property(e => e.Name)
                .HasMaxLength(35)
                .HasColumnName("name");
            entity.Property(e => e.Sku)
                .HasMaxLength(35)
                .HasColumnName("sku");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.Stockleft).HasColumnName("stockleft");
        });

        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.HasKey(e => e.Warehouseid).HasName("warehouse_pkey");

            entity.ToTable("warehouse");

            entity.HasIndex(e => e.Warehousename, "warehouse_warehousename_key").IsUnique();

            entity.Property(e => e.Warehouseid)
                .UseIdentityAlwaysColumn()
                .HasColumnName("warehouseid");
            entity.Property(e => e.Businessid).HasColumnName("businessid");
            entity.Property(e => e.Warehousename)
                .HasMaxLength(50)
                .HasColumnName("warehousename");

            entity.HasOne(d => d.Business).WithMany(p => p.Warehouses)
                .HasForeignKey(d => d.Businessid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_business");
        });

        modelBuilder.Entity<Warehouseproduct>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("warehouseproduct");

            entity.Property(e => e.Businessid).HasColumnName("businessid");
            entity.Property(e => e.Productid).HasColumnName("productid");
            entity.Property(e => e.Warehouseid).HasColumnName("warehouseid");

            entity.HasOne(d => d.Product).WithMany()
                .HasForeignKey(d => d.Productid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_prod_prod");

            entity.HasOne(d => d.Warehouse).WithMany()
                .HasForeignKey(d => d.Warehouseid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_prod_warehouse");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
