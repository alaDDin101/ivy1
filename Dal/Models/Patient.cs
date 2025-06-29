using System;
using System.Collections.Generic;

namespace Dal.Models;

public partial class Patient
{
    public long Person { get; set; }

    public string PatientCode { get; set; } = null!;

    public virtual Person PersonNavigation { get; set; } = null!;
}
