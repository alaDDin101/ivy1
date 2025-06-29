using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class User
{
    public long Party { get; set; }

    public string MembershipUser { get; set; } = null!;

    public virtual Party PartyNavigation { get; set; } = null!;
}
