using System;
using System.Collections.Generic;

namespace EquipLink.Models;

public partial class Request
{
    public int ReqId { get; set; }

    public int UserId { get; set; }

    public int EquId { get; set; }

    public string ReqDescription { get; set; } = null!;

    public DateOnly ReqDate { get; set; }

    public string ReqApprovalStatus { get; set; } = null!;

    public string? ReqAdminNotes { get; set; }

    public decimal? ReqInsurancePerDay { get; set; }

    public virtual Equipment Equ { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
