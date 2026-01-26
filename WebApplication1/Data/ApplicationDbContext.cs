using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    /// <summary>
    /// ApplicationDbContext is the main EF Core database context for the application.
    /// It defines all DbSets (tables) and configures relationships between entities.
    /// Handles database operations and enforces unique constraints.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets represent tables in the database
        public DbSet<User> Users { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<Vacancy> Vacancies { get; set; }
        public DbSet<JobApplication> JobApplications { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User Configuration
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Driver Configuration - One-to-One with User
            modelBuilder.Entity<Driver>()
                .HasOne(d => d.User)
                .WithOne(u => u.Driver)
                .HasForeignKey<Driver>(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Driver Configuration - One-to-One with Vehicle
            modelBuilder.Entity<Driver>()
                .HasOne(d => d.Vehicle)
                .WithOne(v => v.Driver)
                .HasForeignKey<Driver>(d => d.AssignedVehicleId)
                .OnDelete(DeleteBehavior.SetNull);

            // Vehicle Configuration - Unique LicensePlate
            modelBuilder.Entity<Vehicle>()
                .HasIndex(v => v.LicensePlate)
                .IsUnique();

            // Vehicle Configuration - Many-to-One with Warehouse
            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Warehouse)
                .WithMany(w => w.Vehicles)
                .HasForeignKey(v => v.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Vacancy Configuration - Many-to-One with Warehouse
            modelBuilder.Entity<Vacancy>()
                .HasOne(v => v.Warehouse)
                .WithMany(w => w.Vacancies)
                .HasForeignKey(v => v.WarehouseId)
                .OnDelete(DeleteBehavior.Cascade);

            // JobApplication Configuration
            modelBuilder.Entity<JobApplication>()
                .HasOne(ja => ja.User)
                .WithMany(u => u.JobApplications)
                .HasForeignKey(ja => ja.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<JobApplication>()
                .HasOne(ja => ja.Vacancy)
                .WithMany(v => v.JobApplications)
                .HasForeignKey(ja => ja.VacancyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Notification Configuration
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ActivityLog Configuration
            modelBuilder.Entity<ActivityLog>()
                .HasOne(al => al.User)
                .WithMany(u => u.ActivityLogs)
                .HasForeignKey(al => al.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}