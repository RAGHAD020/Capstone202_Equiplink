using System;
using System.Collections.Generic;

namespace EquipLink.Models;

public partial class Review
{
    public int RevId { get; set; }

    public int RevRatingValue { get; set; }

    public string? RevComment { get; set; }

    public DateOnly RevDate { get; set; }

    public short? RevIsVerified { get; set; }

    public int? OrdId { get; set; }

    public int CustomerId { get; set; }

    public int ProviderId { get; set; }

    public virtual User Customer { get; set; } = null!;

    public virtual Order? Ord { get; set; }

    public virtual User Provider { get; set; } = null!;
}
