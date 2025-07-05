using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Governorate
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<City> Cities { get; set; } = new List<City>();
}
