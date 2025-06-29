using System;
using System.Collections.Generic;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public partial class IvyContext : DbContext
{
    public IvyContext()
    {
    }

    public IvyContext(DbContextOptions<IvyContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<AppointmentNote> AppointmentNotes { get; set; }

    public virtual DbSet<AppointmentStatus> AppointmentStatuses { get; set; }

    public virtual DbSet<Billing> Billings { get; set; }

    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<Clinic> Clinics { get; set; }

    public virtual DbSet<Doctor> Doctors { get; set; }

    public virtual DbSet<DoctorClinic> DoctorClinics { get; set; }

    public virtual DbSet<Party> Parties { get; set; }

    public virtual DbSet<Patient> Patients { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    public virtual DbSet<PaymentStatus> PaymentStatuses { get; set; }

    public virtual DbSet<Person> People { get; set; }

    public virtual DbSet<Specialty> Specialties { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=Ivy;User Id=sa;Password=A@123456789;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Arabic_CI_AS");

        modelBuilder.Entity<Address>(entity =>
        {
            entity.ToTable("Address");

            entity.Property(e => e.DetailedAddress)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.CityNavigation).WithMany(p => p.Addresses)
                .HasForeignKey(d => d.City)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Address_City");

            entity.HasOne(d => d.ClinicNavigation).WithMany(p => p.Addresses)
                .HasForeignKey(d => d.Clinic)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Address_Clinic");
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.ToTable("Appointment");

            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.Reason)
                .HasMaxLength(1000)
                .IsUnicode(false);

            entity.HasOne(d => d.DoctorClinicNavigation).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.DoctorClinic)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Appointment_DoctorClinic");

            entity.HasOne(d => d.StatusNavigation).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.Status)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Appointment_AppointmentStatus");
        });

        modelBuilder.Entity<AppointmentNote>(entity =>
        {
            entity.HasKey(e => e.Appointment);

            entity.Property(e => e.Appointment).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Notes)
                .HasMaxLength(2000)
                .IsUnicode(false);

            entity.HasOne(d => d.AppointmentNavigation).WithOne(p => p.AppointmentNote)
                .HasForeignKey<AppointmentNote>(d => d.Appointment)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AppointmentNotes_Appointment");

            entity.HasOne(d => d.DoctorNavigation).WithMany(p => p.AppointmentNotes)
                .HasForeignKey(d => d.Doctor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AppointmentNotes_Doctor");
        });

        modelBuilder.Entity<AppointmentStatus>(entity =>
        {
            entity.ToTable("AppointmentStatus");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Billing>(entity =>
        {
            entity.ToTable("Billing");

            entity.Property(e => e.Currency)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.Notes)
                .HasMaxLength(450)
                .IsUnicode(false);

            entity.HasOne(d => d.AppointmentNavigation).WithMany(p => p.Billings)
                .HasForeignKey(d => d.Appointment)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Billing_Appointment");

            entity.HasOne(d => d.PaymentMethodNavigation).WithMany(p => p.Billings)
                .HasForeignKey(d => d.PaymentMethod)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Billing_PaymentMethod");

            entity.HasOne(d => d.PaymentStatusNavigation).WithMany(p => p.Billings)
                .HasForeignKey(d => d.PaymentStatus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Billing_PaymentStatus");
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.ToTable("City");

            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Clinic>(entity =>
        {
            entity.ToTable("Clinic");

            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(e => e.Person);

            entity.ToTable("Doctor");

            entity.Property(e => e.Person).ValueGeneratedNever();
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .IsUnicode(false);
            entity.Property(e => e.Image)
                .HasMaxLength(2000)
                .IsUnicode(false);

            entity.HasOne(d => d.PersonNavigation).WithOne(p => p.Doctor)
                .HasForeignKey<Doctor>(d => d.Person)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Doctor_Person");

            entity.HasOne(d => d.SpecialtyNavigation).WithMany(p => p.Doctors)
                .HasForeignKey(d => d.Specialty)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Doctor_Specialty");
        });

        modelBuilder.Entity<DoctorClinic>(entity =>
        {
            entity.ToTable("DoctorClinic");

            entity.HasOne(d => d.ClinicNavigation).WithMany(p => p.DoctorClinics)
                .HasForeignKey(d => d.Clinic)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DoctorClinic_Clinic");

            entity.HasOne(d => d.DoctorNavigation).WithMany(p => p.DoctorClinics)
                .HasForeignKey(d => d.Doctor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DoctorClinic_Doctor");
        });

        modelBuilder.Entity<Party>(entity =>
        {
            entity.ToTable("Party");

            entity.Property(e => e.DispalyName)
                .HasMaxLength(250)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.Person);

            entity.ToTable("Patient");

            entity.Property(e => e.Person).ValueGeneratedNever();
            entity.Property(e => e.PatientCode)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.PersonNavigation).WithOne(p => p.Patient)
                .HasForeignKey<Patient>(d => d.Person)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Patient_Person");
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.ToTable("PaymentMethod");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<PaymentStatus>(entity =>
        {
            entity.ToTable("PaymentStatus");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasKey(e => e.Party);

            entity.ToTable("Person");

            entity.Property(e => e.Party).ValueGeneratedNever();
            entity.Property(e => e.Address)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.FatherName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MotherName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NationalNumber)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.PartyNavigation).WithOne(p => p.Person)
                .HasForeignKey<Person>(d => d.Party)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Person_Party");
        });

        modelBuilder.Entity<Specialty>(entity =>
        {
            entity.ToTable("Specialty");

            entity.Property(e => e.EnName)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .IsUnicode(false);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Party);

            entity.ToTable("User");

            entity.Property(e => e.Party).ValueGeneratedNever();
            entity.Property(e => e.MembershipUser)
                .HasMaxLength(500)
                .IsUnicode(false);

            entity.HasOne(d => d.PartyNavigation).WithOne(p => p.User)
                .HasForeignKey<User>(d => d.Party)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_Party");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
