using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Person
{
    public long Party { get; set; }

    public string FirstName { get; set; } = null!;

    public string FatherName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? MotherName { get; set; }

    public DateOnly? BirthDate { get; set; }

    public string? Address { get; set; }

    public string NationalNumber { get; set; } = null!;

    public virtual Doctor? Doctor { get; set; }

    public virtual Party PartyNavigation { get; set; } = null!;

    public virtual Patient? Patient { get; set; }
    public virtual ICollection<ClinicEmplyee> ClinicEmplyees { get; set; } = new List<ClinicEmplyee>();
}
