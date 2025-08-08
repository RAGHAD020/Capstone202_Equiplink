using System;
using System.Collections.Generic;

namespace EquipLink.Models;

public partial class Address
{
    public int AddId { get; set; }

    public string AddStreet { get; set; } = null!;

    public string AddCity { get; set; } = null!;

    public string? AddState { get; set; }

    public string? AddPostalCode { get; set; }

    public string AddCountry { get; set; } = null!;

    public int UserId { get; set; }

    public short? AddIsDefault { get; set; }

    public virtual ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();

    public virtual User User { get; set; } = null!;
}
