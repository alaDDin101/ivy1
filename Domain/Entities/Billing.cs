using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Billing
{
    public long Id { get; set; }

    public long Appointment { get; set; }

    public double Amount { get; set; }

    public DateTime Date { get; set; }

    public int PaymentStatus { get; set; }

    public int PaymentMethod { get; set; }

    public string Notes { get; set; } = null!;

    public string Currency { get; set; } = null!;

    public virtual Appointment AppointmentNavigation { get; set; } = null!;

    public virtual PaymentMethod PaymentMethodNavigation { get; set; } = null!;

    public virtual PaymentStatus PaymentStatusNavigation { get; set; } = null!;
}
