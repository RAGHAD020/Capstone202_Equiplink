using System;
using System.Collections.Generic;

namespace EquipLink.Models;

public partial class Delivery
{
    public int DeliverId { get; set; }

    public DateTime DeliverDate { get; set; }

    public string? DeliverStatus { get; set; }

    public decimal? DeliverFee { get; set; }

    public string? DeliverAt { get; set; }

    public string? DeliverNote { get; set; }

    public string? DeliverProviderName { get; set; }

    public int AddId { get; set; }

    public int OrdId { get; set; }

    public virtual Address Add { get; set; } = null!;

    public virtual Order Ord { get; set; } = null!;
}
