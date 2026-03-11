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

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<CommandItem> CommandItems { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Kardex> Kardices { get; set; }

    public virtual DbSet<OrderCommand> OrderCommands { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<OrderStatus> OrderStatuses { get; set; }

    public virtual DbSet<OrderTicket> OrderTickets { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PaymentType> PaymentTypes { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductStatus> ProductStatuses { get; set; }

    public virtual DbSet<Station> Stations { get; set; }

    public virtual DbSet<StationType> StationTypes { get; set; }

    public virtual DbSet<Unit> Units { get; set; }

    public virtual DbSet<Waiter> Waiters { get; set; }

    public virtual DbSet<Warehouse> Warehouses { get; set; }

    public virtual DbSet<WarehouseProduct> WarehouseProducts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Business>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("business_pkey");

            entity.ToTable("business");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("category_pkey");

            entity.ToTable("category");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<CommandItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("command_item_pkey");

            entity.ToTable("command_item");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CommandId).HasColumnName("command_id");
            entity.Property(e => e.OrderItemId).HasColumnName("order_item_id");
            entity.Property(e => e.StationId).HasColumnName("station_id");

            entity.HasOne(d => d.OrderItem).WithMany(p => p.CommandItems)
                .HasForeignKey(d => d.OrderItemId)
                .HasConstraintName("command_item_order_item_id_fkey");

            entity.HasOne(d => d.Station).WithMany(p => p.CommandItems)
                .HasForeignKey(d => d.StationId)
                .HasConstraintName("command_item_station_id_fkey");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("customer_pkey");

            entity.ToTable("customer");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Phone)
                .HasMaxLength(50)
                .HasColumnName("phone");
        });

        modelBuilder.Entity<Kardex>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("kardex_pkey");

            entity.ToTable("kardex");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActionQty).HasColumnName("action_qty");
            entity.Property(e => e.ActionType)
                .HasMaxLength(50)
                .HasColumnName("action_type");
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

            entity.HasOne(d => d.Warehouse).WithMany(p => p.Kardices)
                .HasForeignKey(d => d.WarehouseId)
                .HasConstraintName("kardex_warehouse_id_fkey");
        });

        modelBuilder.Entity<OrderCommand>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("order_command_pkey");

            entity.ToTable("order_command");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.WaiterId).HasColumnName("waiter_id");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderCommands)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("order_command_order_id_fkey");

            entity.HasOne(d => d.Waiter).WithMany(p => p.OrderCommands)
                .HasForeignKey(d => d.WaiterId)
                .HasConstraintName("order_command_waiter_id_fkey");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("order_item_pkey");

            entity.ToTable("order_item");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AdditionalNote)
                .HasMaxLength(255)
                .HasColumnName("additional_note");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Qty).HasColumnName("qty");
            entity.Property(e => e.StatusId).HasColumnName("status_id");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("order_item_order_id_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("order_item_product_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.StatusId)
                .HasConstraintName("order_item_status_id_fkey");
        });

        modelBuilder.Entity<OrderStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("order_status_pkey");

            entity.ToTable("order_status");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<OrderTicket>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("order_ticket_pkey");

            entity.ToTable("order_ticket");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.StatusId).HasColumnName("status_id");

            entity.HasOne(d => d.Customer).WithMany(p => p.OrderTickets)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("order_ticket_customer_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.OrderTickets)
                .HasForeignKey(d => d.StatusId)
                .HasConstraintName("order_ticket_status_id_fkey");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("payment_pkey");

            entity.ToTable("payment");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.PaidAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("paid_at");
            entity.Property(e => e.PaymentTypeId).HasColumnName("payment_type_id");

            entity.HasOne(d => d.Order).WithMany(p => p.Payments)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("payment_order_id_fkey");

            entity.HasOne(d => d.PaymentType).WithMany(p => p.Payments)
                .HasForeignKey(d => d.PaymentTypeId)
                .HasConstraintName("payment_payment_type_id_fkey");
        });

        modelBuilder.Entity<PaymentType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("payment_type_pkey");

            entity.ToTable("payment_type");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("product_pkey");

            entity.ToTable("product");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.UnitId).HasColumnName("unit_id");
            entity.Property(e => e.UnitQty).HasColumnName("unit_qty");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("product_category_id_fkey");

            entity.HasOne(d => d.Unit).WithMany(p => p.Products)
                .HasForeignKey(d => d.UnitId)
                .HasConstraintName("product_unit_id_fkey");
        });

        modelBuilder.Entity<ProductStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("product_status_pkey");

            entity.ToTable("product_status");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Station>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("station_pkey");

            entity.ToTable("station");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.TypeId).HasColumnName("type_id");

            entity.HasOne(d => d.Type).WithMany(p => p.Stations)
                .HasForeignKey(d => d.TypeId)
                .HasConstraintName("station_type_id_fkey");
        });

        modelBuilder.Entity<StationType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("station_type_pkey");

            entity.ToTable("station_type");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");

            entity.HasMany(d => d.Categories).WithMany(p => p.StationTypes)
                .UsingEntity<Dictionary<string, object>>(
                    "StationCoverage",
                    r => r.HasOne<Category>().WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("station_coverage_category_id_fkey"),
                    l => l.HasOne<StationType>().WithMany()
                        .HasForeignKey("StationTypeId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("station_coverage_station_type_id_fkey"),
                    j =>
                    {
                        j.HasKey("StationTypeId", "CategoryId").HasName("station_coverage_pkey");
                        j.ToTable("station_coverage");
                        j.IndexerProperty<int>("StationTypeId").HasColumnName("station_type_id");
                        j.IndexerProperty<int>("CategoryId").HasColumnName("category_id");
                    });
        });

        modelBuilder.Entity<Unit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("unit_pkey");

            entity.ToTable("unit");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Waiter>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("waiter_pkey");

            entity.ToTable("waiter");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Phone)
                .HasMaxLength(50)
                .HasColumnName("phone");
        });

        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("warehouse_pkey");

            entity.ToTable("warehouse");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BusinessId).HasColumnName("business_id");
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

            entity.ToTable("warehouse_product");

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

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
