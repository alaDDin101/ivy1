using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Clinic
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string Description { get; set; } = null!;

    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();

    public virtual ICollection<DoctorClinic> DoctorClinics { get; set; } = new List<DoctorClinic>();
    public virtual ICollection<ClinicEmplyee> ClinicEmplyees { get; set; } = new List<ClinicEmplyee>();
}
