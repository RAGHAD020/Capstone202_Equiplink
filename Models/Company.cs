using System;
using System.Collections.Generic;

namespace EquipLink.Models;

public partial class Company
{
    public int CoId { get; set; }

    public string CoName { get; set; } = null!;

    public string? CoEmail { get; set; }

    public string? CoPhone { get; set; }

    public string? CoTaxNumber { get; set; }

    public int UserId { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
