using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class DoctorClinic
{
    public long Id { get; set; }

    public long Doctor { get; set; }

    public long Clinic { get; set; }

    public DateOnly From { get; set; }

    public DateOnly? To { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual Clinic ClinicNavigation { get; set; } = null!;

    public virtual Doctor DoctorNavigation { get; set; } = null!;
}
