using MedConnect.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MedConnect.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Facility> Facilities => Set<Facility>();
    public DbSet<Visit> Visits => Set<Visit>();
    public DbSet<Referral> Referrals => Set<Referral>();
    public DbSet<FieldVisitLog> FieldVisitLogs => Set<FieldVisitLog>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<VitalSign> VitalSigns => Set<VitalSign>();
    public DbSet<Division> Divisions => Set<Division>();
    public DbSet<LabResult> LabResults => Set<LabResult>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<AuditLogEntry> AuditLogs => Set<AuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ChatMessage>()
            .HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ChatMessage>()
            .HasOne(m => m.Receiver)
            .WithMany()
            .HasForeignKey(m => m.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Patient>()
            .HasIndex(p => p.NHISNumber)
            .IsUnique();

        builder.Entity<Patient>()
            .HasIndex(p => p.UserId)
            .IsUnique();

        builder.Entity<Patient>()
            .HasOne(p => p.User)
            .WithOne()
            .HasForeignKey<Patient>(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ApplicationUser>()
            .HasOne(u => u.Facility)
            .WithMany(f => f.Users)
            .HasForeignKey(u => u.FacilityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ApplicationUser>()
            .HasOne(u => u.Division)
            .WithMany(d => d.Doctors)
            .HasForeignKey(u => u.DivisionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Division>()
            .HasIndex(d => d.Name)
            .IsUnique();

        builder.Entity<Visit>()
            .HasOne(v => v.Patient)
            .WithMany(p => p.Visits)
            .HasForeignKey(v => v.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Visit>()
            .HasOne(v => v.Facility)
            .WithMany(f => f.Visits)
            .HasForeignKey(v => v.FacilityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Referral>()
            .HasOne(r => r.Patient)
            .WithMany(p => p.Referrals)
            .HasForeignKey(r => r.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Referral>()
            .HasOne(r => r.FromFacility)
            .WithMany()
            .HasForeignKey(r => r.FromFacilityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Referral>()
            .HasOne(r => r.ToFacility)
            .WithMany()
            .HasForeignKey(r => r.ToFacilityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<FieldVisitLog>()
            .Property(l => l.ChildWeightKg)
            .HasPrecision(5, 2);

        builder.Entity<FieldVisitLog>()
            .HasOne(l => l.CHWUser)
            .WithMany(u => u.FieldVisitLogs)
            .HasForeignKey(l => l.CHWUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<FieldVisitLog>()
            .HasOne(l => l.Patient)
            .WithMany(p => p.FieldVisitLogs)
            .HasForeignKey(l => l.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Appointment>()
            .HasOne(a => a.Patient)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Appointment>()
            .HasOne(a => a.Facility)
            .WithMany()
            .HasForeignKey(a => a.FacilityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Appointment>()
            .HasOne(a => a.CreatedByUser)
            .WithMany(u => u.CreatedAppointments)
            .HasForeignKey(a => a.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<VitalSign>()
            .HasOne(v => v.Patient)
            .WithMany(p => p.VitalSigns)
            .HasForeignKey(v => v.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<VitalSign>()
            .HasOne(v => v.RecordedByUser)
            .WithMany()
            .HasForeignKey(v => v.RecordedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<LabResult>()
            .HasOne(l => l.Patient)
            .WithMany()
            .HasForeignKey(l => l.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<LabResult>()
            .HasOne(l => l.Facility)
            .WithMany()
            .HasForeignKey(l => l.FacilityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<LabResult>()
            .HasOne(l => l.OrderedByUser)
            .WithMany()
            .HasForeignKey(l => l.OrderedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<LabResult>()
            .HasOne(l => l.ReviewedByUser)
            .WithMany()
            .HasForeignKey(l => l.ReviewedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Prescription>()
            .HasOne(p => p.Patient)
            .WithMany()
            .HasForeignKey(p => p.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Prescription>()
            .HasOne(p => p.Visit)
            .WithMany()
            .HasForeignKey(p => p.VisitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Prescription>()
            .HasOne(p => p.PrescribedByUser)
            .WithMany()
            .HasForeignKey(p => p.PrescribedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<AuditLogEntry>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Action).HasMaxLength(100);
            e.Property(a => a.EntityType).HasMaxLength(100);
            e.Property(a => a.EntityId).HasMaxLength(64);
            e.Property(a => a.ActorUserId).HasMaxLength(450);
            e.Property(a => a.ActorEmail).HasMaxLength(256);
            e.HasIndex(a => a.PerformedAtUtc);
        });
    }
}
