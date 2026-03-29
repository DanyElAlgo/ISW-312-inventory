using Microsoft.EntityFrameworkCore;

namespace Sales.API.Models;

public partial class SalesDbContext : DbContext
{
    public SalesDbContext()
    {
    }

    public SalesDbContext(DbContextOptions<SalesDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Customer> Customers { get; set; }
    public virtual DbSet<GlobalTaxConfig> GlobalTaxConfigs { get; set; }
    public virtual DbSet<OrderCommand> OrderCommands { get; set; }
    public virtual DbSet<CommandItem> CommandItems { get; set; }
    public virtual DbSet<OrderItem> OrderItems { get; set; }
    public virtual DbSet<OrderStatus> OrderStatuses { get; set; }
    public virtual DbSet<OrderTicket> OrderTickets { get; set; }
    public virtual DbSet<Payment> Payments { get; set; }
    public virtual DbSet<PaymentType> PaymentTypes { get; set; }
    public virtual DbSet<Station> Stations { get; set; }
    public virtual DbSet<StationType> StationTypes { get; set; }
    public virtual DbSet<Waiter> Waiters { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        const string schema = "sales";

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("customer_pkey");
            entity.ToTable("customer", schema);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasMaxLength(255).HasColumnName("name");
            entity.Property(e => e.Phone).HasMaxLength(50).HasColumnName("phone");
        });

        modelBuilder.Entity<GlobalTaxConfig>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("global_tax_config_pkey");
            entity.ToTable("global_tax_config", schema);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TaxRate)
                .HasColumnType("numeric(7,4)")
                .HasColumnName("tax_rate");
        });

        modelBuilder.Entity<OrderStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("order_status_pkey");
            entity.ToTable("order_status", schema);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description).HasMaxLength(255).HasColumnName("description");
            entity.Property(e => e.Name).HasMaxLength(255).HasColumnName("name");
        });

        modelBuilder.Entity<OrderTicket>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("order_ticket_pkey");
            entity.ToTable("order_ticket", schema);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.TaxRateSnapshot)
                .HasColumnType("numeric(7,4)")
                .HasDefaultValue(0m)
                .HasColumnName("tax_rate_snapshot");

            entity.HasOne(d => d.Customer).WithMany(p => p.OrderTickets)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("order_ticket_customer_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.OrderTickets)
                .HasForeignKey(d => d.StatusId)
                .HasConstraintName("order_ticket_status_id_fkey");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("order_item_pkey");
            entity.ToTable("order_item", schema);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AdditionalNote).HasMaxLength(255).HasColumnName("additional_note");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            // product_id is a logical reference only — no EF navigation, no DB-level FK
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ProductName).HasMaxLength(255).HasColumnName("product_name");
            entity.Property(e => e.UnitPrice).HasColumnType("numeric(12,2)").HasColumnName("unit_price");
            entity.Property(e => e.Qty).HasColumnName("qty");
            entity.Property(e => e.StatusId).HasColumnName("status_id");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("order_item_order_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.StatusId)
                .HasConstraintName("order_item_status_id_fkey");
        });

        modelBuilder.Entity<OrderCommand>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("order_command_pkey");
            entity.ToTable("order_command", schema);
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

        modelBuilder.Entity<CommandItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("command_item_pkey");
            entity.ToTable("command_item", schema);
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

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("payment_pkey");
            entity.ToTable("payment", schema);
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
            entity.ToTable("payment_type", schema);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description).HasMaxLength(255).HasColumnName("description");
            entity.Property(e => e.Name).HasMaxLength(255).HasColumnName("name");
        });

        modelBuilder.Entity<StationType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("station_type_pkey");
            entity.ToTable("station_type", schema);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description).HasMaxLength(255).HasColumnName("description");
            entity.Property(e => e.Name).HasMaxLength(255).HasColumnName("name");
            // CategoryIds is not mapped to the DB — it is populated from station_coverage via raw query
            entity.Ignore(e => e.CategoryIds);
        });

        modelBuilder.Entity<Station>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("station_pkey");
            entity.ToTable("station", schema);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasMaxLength(255).HasColumnName("name");
            entity.Property(e => e.TypeId).HasColumnName("type_id");

            entity.HasOne(d => d.Type).WithMany(p => p.Stations)
                .HasForeignKey(d => d.TypeId)
                .HasConstraintName("station_type_id_fkey");
        });

        modelBuilder.Entity<Waiter>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("waiter_pkey");
            entity.ToTable("waiter", schema);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasMaxLength(255).HasColumnName("name");
            entity.Property(e => e.Phone).HasMaxLength(50).HasColumnName("phone");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
