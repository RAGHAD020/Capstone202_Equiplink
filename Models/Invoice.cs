using System;
using System.Collections.Generic;

namespace EquipLink.Models;

public partial class Invoice
{
    public int InvId { get; set; }

    public DateOnly InvDate { get; set; }

    public decimal InvAmountPaid { get; set; }

    public string InvInvoiceNumber { get; set; } = null!;

    public int PayId { get; set; }

    public short? OrdInstallationOperationFee { get; set; }

    public bool InvInstallationOperation { get; set; }

    public virtual Payment Pay { get; set; } = null!;
}
