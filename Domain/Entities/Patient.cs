using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Patient
{
    public long Person { get; set; }

    public string PatientCode { get; set; } = null!;

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual Person PersonNavigation { get; set; } = null!;
}
