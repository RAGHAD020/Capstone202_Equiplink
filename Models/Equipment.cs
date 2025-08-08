using System;
using System.Collections.Generic;

namespace EquipLink.Models;

public partial class Equipment
{
    public int EquId { get; set; }

    public string EquName { get; set; } = null!;

    public string? EquDescription { get; set; }

    public string EquCondition { get; set; } = null!;

    public string EquAvailabilityStatus { get; set; } = null!;

    public int EquQuantity { get; set; }

    public decimal EquPrice { get; set; }

    public string? EquImage { get; set; }

    public DateTime? EquCreatedDate { get; set; }

    public short? EquIsActive { get; set; }

    public int CategId { get; set; }

    public int ProviderId { get; set; }

    public string EquType { get; set; } = null!;

    public byte[] Version { get; set; } = null!;

    public string? EquModel { get; set; }

    public int? EquModelYear { get; set; }

    public string? EquBrand { get; set; }

    public int? EquWorkingHours { get; set; }

    public string? Features { get; set; }

    public string? EquFeatures { get; set; }

    public virtual Category Categ { get; set; } = null!;

    public virtual ICollection<Orderequipment> Orderequipments { get; set; } = new List<Orderequipment>();

    public virtual User Provider { get; set; } = null!;

    public virtual ICollection<Request> Requests { get; set; } = new List<Request>();
}
