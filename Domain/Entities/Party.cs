using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Party
{
    public long Id { get; set; }

    public string DispalyName { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual Person? Person { get; set; }

    public virtual User? User { get; set; }
}
