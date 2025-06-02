using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WorkshopManager.API.Models;

namespace WorkshopManager.API.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<Vehicle> Vehicles { get; set; } = null!;
    public DbSet<ServiceOrder> ServiceOrders { get; set; } = null!;
    public DbSet<ServiceTask> ServiceTasks { get; set; } = null!;
    public DbSet<Part> Parts { get; set; } = null!;
    public DbSet<UsedPart> UsedParts { get; set; } = null!;
    public DbSet<Comment> Comments { get; set; } = null!;
    public DbSet<FileUpload> FileUploads { get; set; } = null!;
    public DbSet<Report> Reports { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure decimal precision
        builder.Entity<Part>()
            .Property(p => p.UnitPrice)
            .HasPrecision(10, 2);

        builder.Entity<ServiceTask>()
            .Property(t => t.LaborCost)
            .HasPrecision(10, 2);

        // Configure relationships
        builder.Entity<Customer>()
            .HasMany(c => c.Vehicles)
            .WithOne(v => v.Customer)
            .HasForeignKey(v => v.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Vehicle>()
            .HasMany(v => v.ServiceOrders)
            .WithOne(o => o.Vehicle)
            .HasForeignKey(o => o.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ServiceOrder>()
            .HasMany(o => o.Tasks)
            .WithOne(t => t.ServiceOrder)
            .HasForeignKey(t => t.ServiceOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ServiceOrder>()
            .HasMany(o => o.Comments)
            .WithOne(c => c.ServiceOrder)
            .HasForeignKey(c => c.ServiceOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ServiceTask>()
            .HasMany(t => t.UsedParts)
            .WithOne(p => p.ServiceTask)
            .HasForeignKey(p => p.ServiceTaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UsedPart>()
            .HasOne(up => up.ServiceTask)
            .WithMany(st => st.UsedParts)
            .HasForeignKey(up => up.ServiceTaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UsedPart>()
            .HasOne(up => up.Part)
            .WithMany(p => p.UsedInTasks)
            .HasForeignKey(up => up.PartId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Comment>()
            .HasOne(c => c.ServiceOrder)
            .WithMany(so => so.Comments)
            .HasForeignKey(c => c.ServiceOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Report>()
            .HasOne(r => r.Vehicle)
            .WithMany()
            .HasForeignKey(r => r.VehicleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Report>()
            .HasOne(r => r.ServiceOrder)
            .WithMany()
            .HasForeignKey(r => r.ServiceOrderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Report>()
            .HasOne(r => r.Customer)
            .WithMany()
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.Entity<Customer>()
            .HasIndex(c => c.Email)
            .IsUnique();

        builder.Entity<Vehicle>()
            .HasIndex(v => v.VIN)
            .IsUnique();

        builder.Entity<Vehicle>()
            .HasIndex(v => v.RegistrationNumber)
            .IsUnique();

        builder.Entity<Part>()
            .HasIndex(p => p.PartNumber)
            .IsUnique();
    }
} 