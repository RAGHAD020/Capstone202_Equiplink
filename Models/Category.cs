using System;
using System.Collections.Generic;

namespace EquipLink.Models;

public partial class Category
{
    public int CategId { get; set; }

    public string CategType { get; set; } = null!;

    public string? CategDescription { get; set; }

    public short? CategIsActive { get; set; }

    public virtual ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();
}
