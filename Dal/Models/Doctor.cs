using System;
using System.Collections.Generic;

namespace Dal.Models;

public partial class Doctor
{
    public long Person { get; set; }

    public string? Description { get; set; }

    public string? Image { get; set; }

    public int Specialty { get; set; }

    public virtual ICollection<AppointmentNote> AppointmentNotes { get; set; } = new List<AppointmentNote>();

    public virtual ICollection<DoctorClinic> DoctorClinics { get; set; } = new List<DoctorClinic>();

    public virtual Person PersonNavigation { get; set; } = null!;

    public virtual Specialty SpecialtyNavigation { get; set; } = null!;
}
