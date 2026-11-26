using Appointment.Api.Entities;
using Microsoft.EntityFrameworkCore;
using AppointmentEntity = global::Appointment.Api.Entities.Appointment;

namespace Appointment.Api.Data;

public class MedicalQueueDbContext : DbContext
{
    public MedicalQueueDbContext(
        DbContextOptions<MedicalQueueDbContext> options)
        : base(options)
    {
    }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Slot> Slots => Set<Slot>();
    public DbSet<AppointmentEntity> Appointments => Set<AppointmentEntity>();
    public DbSet<QueueNumber> QueueNumbers => Set<QueueNumber>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Patient>()
            .HasMany(p => p.Appointments)
            .WithOne(a => a.Patient)
            .HasForeignKey(a => a.PatientId);

        modelBuilder.Entity<Doctor>()
            .HasMany(d => d.Slots)
            .WithOne(s => s.Doctor)
            .HasForeignKey(s => s.DoctorId);

        modelBuilder.Entity<Doctor>()
            .HasMany<AppointmentEntity>()
            .WithOne(a => a.Doctor)
            .HasForeignKey(a => a.DoctorId);

        modelBuilder.Entity<Slot>()
            .HasMany(s => s.Appointments)
            .WithOne(a => a.Slot)
            .HasForeignKey(a => a.SlotId);

        modelBuilder.Entity<AppointmentEntity>()
            .HasOne(a => a.QueueNumber)
            .WithOne(q => q.Appointment)
            .HasForeignKey<QueueNumber>(q => q.AppointmentId);

        modelBuilder.Entity<AppointmentEntity>()
            .HasIndex(a => a.SlotId)
            .IsUnique();

        modelBuilder.Entity<QueueNumber>()
            .HasIndex(q => new { q.DoctorId, q.Number })
            .IsUnique();
    }
}
