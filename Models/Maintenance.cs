using System;
using System.Collections.Generic;

namespace EquipLink.Models;

public partial class Maintenance
{
    public int MainId { get; set; }

    public DateOnly MainRegDate { get; set; }

    public string MainDescription { get; set; } = null!;

    public decimal? MainAmount { get; set; }

    public string? MainStatus { get; set; }

    public DateOnly? MainCompletedDate { get; set; }

    public int OrdId { get; set; }

    public int? UserId { get; set; }

    public virtual Order Ord { get; set; } = null!;
}
