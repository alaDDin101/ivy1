using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class PaymentMethod
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Billing> Billings { get; set; } = new List<Billing>();
}
