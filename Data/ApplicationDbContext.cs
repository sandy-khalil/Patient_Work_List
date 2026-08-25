using Microsoft.EntityFrameworkCore;
using PatientWorklist.API.Entities;

namespace PatientWorklist.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Person> Persons => Set<Person>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Study> Studies => Set<Study>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Person>(entity =>
        {
            entity.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(p => p.LastName).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Gender).IsRequired().HasMaxLength(20);
            entity.Property(p => p.Phone).HasMaxLength(20);
            entity.Property(p => p.Email).HasMaxLength(150);
            entity.HasIndex(p => p.Email).IsUnique();

            entity.HasOne(p => p.Patient)
                  .WithOne(p => p.Person)
                  .HasForeignKey<Patient>(p => p.PersonId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.Doctor)
                  .WithOne(d => d.Person)
                  .HasForeignKey<Doctor>(d => d.PersonId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.Property(p => p.MRN).IsRequired().HasMaxLength(50);
            entity.Property(p => p.Status).IsRequired().HasMaxLength(50);
            entity.HasIndex(p => p.MRN).IsUnique();

            entity.HasMany(p => p.Studies)
                  .WithOne(s => s.Patient)
                  .HasForeignKey(s => s.PatientId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.Property(d => d.Specialty).IsRequired().HasMaxLength(100);

            entity.HasMany(d => d.Studies)
                  .WithOne(s => s.Doctor)
                  .HasForeignKey(s => s.DoctorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Study>(entity =>
        {
            entity.Property(s => s.Modality).IsRequired().HasMaxLength(50);
            entity.Property(s => s.Status).IsRequired().HasMaxLength(50);
            entity.HasIndex(s => new { s.PatientId, s.StudyDate });
        });
    }
}