using System;
using System.Collections.Generic;

namespace EquipLink.Models;

public partial class Order
{
    public int OrdId { get; set; }

    public decimal OrdTotalPrice { get; set; }

    public string OrdStatus { get; set; } = null!;

    public DateTime? OrdCreatedDate { get; set; }

    public string? OrdNotes { get; set; } //we alredy delet the order nots

    public int CustomerId { get; set; }

    public int ProviderId { get; set; }

    public short? OrdInstallationOperation { get; set; }

    public virtual User Customer { get; set; } = null!;

    public virtual ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();

    public virtual ICollection<Maintenance> Maintenances { get; set; } = new List<Maintenance>();

    public virtual ICollection<Orderequipment> Orderequipments { get; set; } = new List<Orderequipment>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual User Provider { get; set; } = null!;

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
