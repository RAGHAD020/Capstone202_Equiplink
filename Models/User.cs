using System;
using System.Collections.Generic;

namespace EquipLink.Models;

public partial class User
{
    public int UserId { get; set; }

    public string UserFname { get; set; } = null!;

    public string UserLname { get; set; } = null!;

    public string UserEmail { get; set; } = null!;

    public string? UserPhone { get; set; }

    public string UserPassword { get; set; } = null!;

    public string UserType { get; set; } = null!;

    public short? UserIsActive { get; set; }

    public DateTime? UserCreatedDate { get; set; }

    public DateTime? UserLastLoginDate { get; set; }

    public int? CoId { get; set; }

    public string? UserNationalId { get; set; }

    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();

    public virtual Company? Co { get; set; }

    public virtual ICollection<Company> Companies { get; set; } = new List<Company>();

    public virtual ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();

    public virtual ICollection<Order> OrderCustomers { get; set; } = new List<Order>();

    public virtual ICollection<Order> OrderProviders { get; set; } = new List<Order>();

    public virtual ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();

    public virtual ICollection<Request> Requests { get; set; } = new List<Request>();

    public virtual ICollection<Review> ReviewCustomers { get; set; } = new List<Review>();

    public virtual ICollection<Review> ReviewProviders { get; set; } = new List<Review>();
}
