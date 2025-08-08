using System;
using System.Collections.Generic;

namespace EquipLink.Models;

public partial class Orderequipment
{
    public int OrdEqId { get; set; }

    public int OrdEqQuantity { get; set; }

    public decimal OrdEqUnitPrice { get; set; }

    public decimal OrdEqSubTotal { get; set; }

    public DateOnly? OrdEqStartDate { get; set; }

    public DateOnly? OrdEqEndDate { get; set; }

    public int OrdId { get; set; }

    public int EquId { get; set; }

    public decimal? OrdEqInsuranceFee { get; set; }

    public string? OrdEqInsuranceStatus { get; set; }

    public virtual Equipment Equ { get; set; } = null!;

    public virtual Order Ord { get; set; } = null!;
}
