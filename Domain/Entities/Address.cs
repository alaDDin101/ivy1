using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Address
{
    public long Id { get; set; }

    public int City { get; set; }

    public string DetailedAddress { get; set; } = null!;

    public long Clinic { get; set; }

    public virtual City CityNavigation { get; set; } = null!;

    public virtual Clinic ClinicNavigation { get; set; } = null!;
}
