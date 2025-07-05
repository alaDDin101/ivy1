using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class ClinicEmplyee
{
    public long Id { get; set; }

    public long Clinic { get; set; }

    public long Person { get; set; }

    public DateOnly From { get; set; }

    public DateOnly? To { get; set; }

    public virtual Clinic ClinicNavigation { get; set; } = null!;

    public virtual Person PersonNavigation { get; set; } = null!;
}
