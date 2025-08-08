using System;
using System.Collections.Generic;

namespace EquipLink.Models;

public partial class Payment
{
    public int PayId { get; set; }

    public string PayMethod { get; set; } = null!;

    public DateTime PayDate { get; set; }

    public string PayStatus { get; set; } = null!;

    public decimal PayAmount { get; set; }

    public string? PayTransactionId { get; set; }

    public string? PayNotes { get; set; }

    public int OrdId { get; set; }

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual Order Ord { get; set; } = null!;
}
