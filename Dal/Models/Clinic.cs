using System;
using System.Collections.Generic;

namespace Dal.Models;

public partial class Clinic
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();

    public virtual ICollection<DoctorClinic> DoctorClinics { get; set; } = new List<DoctorClinic>();
}
