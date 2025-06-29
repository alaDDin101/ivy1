using System;
using System.Collections.Generic;

namespace Dal.Models;

public partial class AppointmentNote
{
    public long Appointment { get; set; }

    public string Notes { get; set; } = null!;

    public long Doctor { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Appointment AppointmentNavigation { get; set; } = null!;

    public virtual Doctor DoctorNavigation { get; set; } = null!;
}
