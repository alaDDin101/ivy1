using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Appointment
{
    public long Id { get; set; }

    public long Patient { get; set; }

    public DateTime Date { get; set; }

    public string Reason { get; set; } = null!;

    public int Status { get; set; }

    public long DoctorClinic { get; set; }

    public virtual AppointmentNote? AppointmentNote { get; set; }

    public virtual ICollection<Billing> Billings { get; set; } = new List<Billing>();

    public virtual DoctorClinic DoctorClinicNavigation { get; set; } = null!;

    public virtual AppointmentStatus StatusNavigation { get; set; } = null!;
}
