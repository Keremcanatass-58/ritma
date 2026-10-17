using Ritma.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Ritma.Api.Data;

public class RitmaDbContext : DbContext
{
    public RitmaDbContext(DbContextOptions<RitmaDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Appointment> Appointments => Set<Appointment>();

    public DbSet<Service> Services => Set<Service>();

    public DbSet<Business> Businesses => Set<Business>();

    public DbSet<Staff> Staff => Set<Staff>();

    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<AppUser> Users => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Service>()
            .Property(s => s.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Payment>()
            .Property(p => p.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Payment>()
            .HasIndex(p => p.AppointmentId)
            .IsUnique();

        modelBuilder.Entity<Payment>()
            .HasIndex(p => new { p.BusinessId, p.CreatedAt });

        modelBuilder.Entity<Payment>()
            .HasIndex(p => new { p.BusinessId, p.Status, p.CreatedAt });

        modelBuilder.Entity<Appointment>()
            .HasIndex(a => new { a.BusinessId, a.StartTime });

        modelBuilder.Entity<Appointment>()
            .HasIndex(a => new { a.BusinessId, a.StaffId, a.StartTime, a.EndTime });

        modelBuilder.Entity<Appointment>()
            .HasIndex(a => a.PublicManageTokenHash)
            .IsUnique()
            .HasFilter("[PublicManageTokenHash] IS NOT NULL");

        modelBuilder.Entity<Customer>()
            .HasIndex(c => new { c.BusinessId, c.Phone });

        modelBuilder.Entity<Customer>()
            .HasIndex(c => new { c.BusinessId, c.LastName });

        modelBuilder.Entity<Staff>()
            .HasIndex(s => new { s.BusinessId, s.IsActive });

        modelBuilder.Entity<Service>()
            .HasIndex(s => new { s.BusinessId, s.IsActive });

        modelBuilder.Entity<AppUser>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Business)
            .WithMany(b => b.Payments)
            .HasForeignKey(p => p.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Customer)
            .WithMany()
            .HasForeignKey(p => p.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Appointment)
            .WithMany()
            .HasForeignKey(p => p.AppointmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AppUser>()
            .HasOne(u => u.Business)
            .WithMany(b => b.Users)
            .HasForeignKey(u => u.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Customer)
            .WithMany()
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Staff)
            .WithMany()
            .HasForeignKey(a => a.StaffId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Service)
            .WithMany()
            .HasForeignKey(a => a.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Business)
            .WithMany(b => b.Appointments)
            .HasForeignKey(a => a.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Customer>()
            .HasOne(c => c.Business)
            .WithMany(b => b.Customers)
            .HasForeignKey(c => c.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Staff>()
            .HasOne(s => s.Business)
            .WithMany(b => b.Staff)
            .HasForeignKey(s => s.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Service>()
            .HasOne(s => s.Business)
            .WithMany(b => b.Services)
            .HasForeignKey(s => s.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
