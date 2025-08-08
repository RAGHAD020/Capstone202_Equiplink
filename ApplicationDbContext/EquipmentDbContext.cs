using System;
using System.Collections.Generic;
using EquipLink.Models;
using Microsoft.EntityFrameworkCore;

namespace EquipLink.ApplicationDbContext;

public partial class EquipmentDbContext : DbContext
{
    public EquipmentDbContext()
    {
    }

    public EquipmentDbContext(DbContextOptions<EquipmentDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<Delivery> Deliveries { get; set; }

    public virtual DbSet<Equipment> Equipment { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<Maintenance> Maintenances { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Orderequipment> Orderequipments { get; set; }

    public virtual DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Request> Requests { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<User> Users { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.AddId);

            entity.ToTable("address");

            entity.HasIndex(e => e.UserId, "IX_address_user");

            entity.Property(e => e.AddId).HasColumnName("Add_ID");
            entity.Property(e => e.AddCity)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Add_City");
            entity.Property(e => e.AddCountry)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasDefaultValue("Egypt")
                .HasColumnName("Add_Country");
            entity.Property(e => e.AddIsDefault)
                .HasDefaultValue((short)1)
                .HasColumnName("Add_IsDefault");
            entity.Property(e => e.AddPostalCode)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("Add_Postal_Code");
            entity.Property(e => e.AddState)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Add_State");
            entity.Property(e => e.AddStreet)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("Add_Street");
            entity.Property(e => e.UserId).HasColumnName("User_ID");

            entity.HasOne(d => d.User).WithMany(p => p.Addresses)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_address_user");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategId);

            entity.ToTable("category");

            entity.HasIndex(e => e.CategType, "UK_category_type").IsUnique();

            entity.Property(e => e.CategId).HasColumnName("Categ_ID");
            entity.Property(e => e.CategDescription)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("Categ_Description");
            entity.Property(e => e.CategIsActive)
                .HasDefaultValue((short)1)
                .HasColumnName("Categ_IsActive");
            entity.Property(e => e.CategType)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Categ_Type");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.CoId);

            entity.ToTable("company");

            entity.Property(e => e.CoId).HasColumnName("Co_ID");
            entity.Property(e => e.CoEmail)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("Co_Email");
            entity.Property(e => e.CoName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("Co_Name");
            entity.Property(e => e.CoPhone)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Co_Phone");
            entity.Property(e => e.CoTaxNumber)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Co_TaxNumber");
            entity.Property(e => e.UserId).HasColumnName("User_ID");

            entity.HasOne(d => d.User).WithMany(p => p.Companies)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_company_user");
        });

        modelBuilder.Entity<Delivery>(entity =>
        {
            entity.HasKey(e => e.DeliverId).HasName("PK__delivery__67C179FD46A8E8DD");

            entity.ToTable("delivery");

            entity.Property(e => e.DeliverId).HasColumnName("Deliver_ID");
            entity.Property(e => e.AddId).HasColumnName("Add_ID");
            entity.Property(e => e.DeliverAt)
                .HasMaxLength(255)
                .HasColumnName("Deliver_At");
            entity.Property(e => e.DeliverDate)
                .HasColumnType("datetime")
                .HasColumnName("Deliver_Date");
            entity.Property(e => e.DeliverFee)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("Deliver_Fee");
            entity.Property(e => e.DeliverNote).HasColumnName("Deliver_Note");
            entity.Property(e => e.DeliverProviderName)
                .HasMaxLength(255)
                .HasColumnName("Deliver_Provider_Name");
            entity.Property(e => e.DeliverStatus)
                .HasMaxLength(50)
                .HasColumnName("Deliver_Status");
            entity.Property(e => e.OrdId).HasColumnName("Ord_ID");

            entity.HasOne(d => d.Add).WithMany(p => p.Deliveries)
                .HasForeignKey(d => d.AddId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Delivery_Address");

            entity.HasOne(d => d.Ord).WithMany(p => p.Deliveries)
                .HasForeignKey(d => d.OrdId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Delivery_Order");
        });

        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.HasKey(e => e.EquId);

            entity.ToTable("equipment");

            entity.HasIndex(e => e.CategId, "IX_equipment_category");

            entity.HasIndex(e => e.ProviderId, "IX_equipment_provider");

            entity.Property(e => e.EquId).HasColumnName("Equ_ID");
            entity.Property(e => e.CategId).HasColumnName("Categ_ID");
            entity.Property(e => e.EquAvailabilityStatus)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("Available")
                .HasColumnName("Equ_Availability_Status");
            entity.Property(e => e.EquBrand)
                .HasMaxLength(100)
                .HasColumnName("Equ_Brand");
            entity.Property(e => e.EquCondition)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("Equ_Condition");
            entity.Property(e => e.EquCreatedDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("Equ_CreatedDate");
            entity.Property(e => e.EquDescription)
                .IsUnicode(false)
                .HasColumnName("Equ_Description");
            entity.Property(e => e.EquFeatures)
                .IsUnicode(false)
                .HasColumnName("Equ_Features");
            entity.Property(e => e.EquImage)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("Equ_Image");
            entity.Property(e => e.EquIsActive)
                .HasDefaultValue((short)1)
                .HasColumnName("Equ_IsActive");
            entity.Property(e => e.EquModel)
                .HasMaxLength(100)
                .HasColumnName("Equ_Model");
            entity.Property(e => e.EquModelYear).HasColumnName("Equ_ModelYear");
            entity.Property(e => e.EquName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Equ_Name");
            entity.Property(e => e.EquPrice)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("Equ_Price");
            entity.Property(e => e.EquQuantity)
                .HasDefaultValue(1)
                .HasColumnName("Equ_Quantity");
            entity.Property(e => e.EquType)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("rent")
                .HasColumnName("Equ_Type");
            entity.Property(e => e.EquWorkingHours).HasColumnName("Equ_WorkingHours");
            entity.Property(e => e.Features).IsUnicode(false);
            entity.Property(e => e.ProviderId).HasColumnName("Provider_ID");
            entity.Property(e => e.Version)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Categ).WithMany(p => p.Equipment)
                .HasForeignKey(d => d.CategId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_equipment_category");

            entity.HasOne(d => d.Provider).WithMany(p => p.Equipment)
                .HasForeignKey(d => d.ProviderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_equipment_provider");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.InvId);

            entity.ToTable("invoice");

            entity.HasIndex(e => e.PayId, "IX_invoice_payment");

            entity.HasIndex(e => e.InvInvoiceNumber, "UK_invoice_number").IsUnique();

            entity.Property(e => e.InvId).HasColumnName("Inv_ID");
            entity.Property(e => e.InvAmountPaid)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("Inv_AmountPaid");
            entity.Property(e => e.InvDate).HasColumnName("Inv_Date");
            entity.Property(e => e.InvInstallationOperation).HasColumnName("Inv_InstallationOperation");
            entity.Property(e => e.InvInvoiceNumber)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Inv_InvoiceNumber");
            entity.Property(e => e.OrdInstallationOperationFee)
                .HasDefaultValue((short)0)
                .HasColumnName("Ord_InstallationOperationFee");
            entity.Property(e => e.PayId).HasColumnName("Pay_ID");

            entity.HasOne(d => d.Pay).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.PayId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_invoice_payment");
        });

        modelBuilder.Entity<Maintenance>(entity =>
        {
            entity.HasKey(e => e.MainId);

            entity.ToTable("maintenance");

            entity.HasIndex(e => e.OrdId, "IX_maintenance_order");

            entity.Property(e => e.MainId).HasColumnName("Main_ID");
            entity.Property(e => e.MainAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("Main_Amount");
            entity.Property(e => e.MainCompletedDate).HasColumnName("Main_CompletedDate");
            entity.Property(e => e.MainDescription)
                .IsUnicode(false)
                .HasColumnName("Main_Description");
            entity.Property(e => e.MainRegDate).HasColumnName("Main_Reg_Date");
            entity.Property(e => e.MainStatus)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("Scheduled")
                .HasColumnName("Main_Status");
            entity.Property(e => e.OrdId).HasColumnName("Ord_ID");
            entity.Property(e => e.UserId).HasColumnName("User_ID");

            entity.HasOne(d => d.Ord).WithMany(p => p.Maintenances)
                .HasForeignKey(d => d.OrdId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_maintenance_order");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrdId);

            entity.ToTable("order");

            entity.HasIndex(e => e.CustomerId, "IX_order_customer");

            entity.HasIndex(e => e.ProviderId, "IX_order_provider");

            entity.Property(e => e.OrdId).HasColumnName("Ord_ID");
            entity.Property(e => e.CustomerId).HasColumnName("Customer_ID");
            entity.Property(e => e.OrdCreatedDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("Ord_CreatedDate");
            entity.Property(e => e.OrdInstallationOperation)
                .HasDefaultValue((short)0)
                .HasColumnName("Ord_InstallationOperation");
            entity.Property(e => e.OrdNotes)
                .IsUnicode(false)
                .HasColumnName("Ord_Notes");
            entity.Property(e => e.OrdStatus)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("Pending")
                .HasColumnName("Ord_Status");
            entity.Property(e => e.OrdTotalPrice)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("Ord_Total_Price");
            entity.Property(e => e.ProviderId).HasColumnName("Provider_ID");

            entity.HasOne(d => d.Customer).WithMany(p => p.OrderCustomers)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_order_customer");

            entity.HasOne(d => d.Provider).WithMany(p => p.OrderProviders)
                .HasForeignKey(d => d.ProviderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_order_provider");
        });

        modelBuilder.Entity<Orderequipment>(entity =>
        {
            entity.HasKey(e => e.OrdEqId);

            entity.ToTable("orderequipment");

            entity.HasIndex(e => new { e.OrdEqStartDate, e.OrdEqEndDate }, "IX_orderequipment_dates");

            entity.HasIndex(e => e.EquId, "IX_orderequipment_equipment");

            entity.HasIndex(e => e.OrdId, "IX_orderequipment_order");

            entity.Property(e => e.OrdEqId).HasColumnName("OrdEq_ID");
            entity.Property(e => e.EquId).HasColumnName("Equ_ID");
            entity.Property(e => e.OrdEqEndDate).HasColumnName("OrdEq_End_Date");
            entity.Property(e => e.OrdEqInsuranceFee)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("OrdEq_InsuranceFee");
            entity.Property(e => e.OrdEqInsuranceStatus)
                .HasMaxLength(20)
                .HasColumnName("OrdEq_InsuranceStatus");
            entity.Property(e => e.OrdEqQuantity).HasColumnName("OrdEq_Quantity");
            entity.Property(e => e.OrdEqStartDate).HasColumnName("OrdEq_Start_Date");
            entity.Property(e => e.OrdEqSubTotal)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("OrdEq_SubTotal");
            entity.Property(e => e.OrdEqUnitPrice)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("OrdEq_UnitPrice");
            entity.Property(e => e.OrdId).HasColumnName("Ord_ID");

            entity.HasOne(d => d.Equ).WithMany(p => p.Orderequipments)
                .HasForeignKey(d => d.EquId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_orderequipment_equipment");

            entity.HasOne(d => d.Ord).WithMany(p => p.Orderequipments)
                .HasForeignKey(d => d.OrdId)
                .HasConstraintName("FK_orderequipment_order");
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__password__3213E83F223D281C");

            entity.ToTable("password_reset_token");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ExpiresAt)
                .HasColumnType("datetime")
                .HasColumnName("expires_at");
            entity.Property(e => e.IsUsed).HasColumnName("is_used");
            entity.Property(e => e.Token)
                .HasMaxLength(255)
                .HasColumnName("token");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.PasswordResetTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_password_reset_token_user");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PayId);

            entity.ToTable("payment");

            entity.HasIndex(e => e.OrdId, "IX_payment_order");

            entity.Property(e => e.PayId).HasColumnName("Pay_ID");
            entity.Property(e => e.OrdId).HasColumnName("Ord_ID");
            entity.Property(e => e.PayAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("Pay_Amount");
            entity.Property(e => e.PayDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("Pay_Date");
            entity.Property(e => e.PayMethod)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("Pay_Method");
            entity.Property(e => e.PayNotes)
                .IsUnicode(false)
                .HasColumnName("Pay_Notes");
            entity.Property(e => e.PayStatus)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("Pending")
                .HasColumnName("Pay_Status");
            entity.Property(e => e.PayTransactionId)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Pay_TransactionID");

            entity.HasOne(d => d.Ord).WithMany(p => p.Payments)
                .HasForeignKey(d => d.OrdId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_payment_order");
        });

        modelBuilder.Entity<Request>(entity =>
        {
            entity.HasKey(e => e.ReqId);

            entity.ToTable("request");

            entity.HasIndex(e => e.EquId, "IX_request_equipment");

            entity.HasIndex(e => e.UserId, "IX_request_user");

            entity.Property(e => e.ReqId).HasColumnName("Req_ID");
            entity.Property(e => e.EquId).HasColumnName("Equ_ID");
            entity.Property(e => e.ReqAdminNotes)
                .IsUnicode(false)
                .HasColumnName("Req_AdminNotes");
            entity.Property(e => e.ReqApprovalStatus)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("Pending")
                .HasColumnName("Req_Approval_Status");
            entity.Property(e => e.ReqDate).HasColumnName("Req_Date");
            entity.Property(e => e.ReqDescription)
                .IsUnicode(false)
                .HasColumnName("Req_Description");
            entity.Property(e => e.ReqInsurancePerDay)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("Req_InsurancePerDay");
            entity.Property(e => e.UserId).HasColumnName("User_ID");

            entity.HasOne(d => d.Equ).WithMany(p => p.Requests)
                .HasForeignKey(d => d.EquId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_request_equipment");

            entity.HasOne(d => d.User).WithMany(p => p.Requests)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_request_user");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.RevId);

            entity.ToTable("review");

            entity.HasIndex(e => e.CustomerId, "IX_review_customer");

            entity.HasIndex(e => e.OrdId, "IX_review_order");

            entity.HasIndex(e => e.ProviderId, "IX_review_provider");

            entity.Property(e => e.RevId).HasColumnName("Rev_ID");
            entity.Property(e => e.CustomerId).HasColumnName("Customer_ID");
            entity.Property(e => e.OrdId).HasColumnName("Ord_ID");
            entity.Property(e => e.ProviderId).HasColumnName("Provider_ID");
            entity.Property(e => e.RevComment)
                .IsUnicode(false)
                .HasColumnName("Rev_Comment");
            entity.Property(e => e.RevDate).HasColumnName("Rev_Date");
            entity.Property(e => e.RevIsVerified)
                .HasDefaultValue((short)0)
                .HasColumnName("Rev_IsVerified");
            entity.Property(e => e.RevRatingValue).HasColumnName("Rev_Rating_Value");

            entity.HasOne(d => d.Customer).WithMany(p => p.ReviewCustomers)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_review_customer");

            entity.HasOne(d => d.Ord).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.OrdId)
                .HasConstraintName("FK_review_order");

            entity.HasOne(d => d.Provider).WithMany(p => p.ReviewProviders)
                .HasForeignKey(d => d.ProviderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_review_provider");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("user");

            entity.HasIndex(e => e.UserEmail, "IX_user_email");

            entity.HasIndex(e => e.UserType, "IX_user_type");

            entity.HasIndex(e => e.UserEmail, "UK_user_email").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("User_ID");
            entity.Property(e => e.CoId).HasColumnName("Co_ID");
            entity.Property(e => e.UserCreatedDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("User_CreatedDate");
            entity.Property(e => e.UserEmail)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("User_Email");
            entity.Property(e => e.UserFname)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("User_FName");
            entity.Property(e => e.UserIsActive)
                .HasDefaultValue((short)1)
                .HasColumnName("User_IsActive");
            entity.Property(e => e.UserLastLoginDate)
                .HasPrecision(0)
                .HasColumnName("User_LastLoginDate");
            entity.Property(e => e.UserLname)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("User_LName");
            entity.Property(e => e.UserNationalId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("User_National_ID");
            entity.Property(e => e.UserPassword)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("User_Password");
            entity.Property(e => e.UserPhone)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("User_Phone");
            entity.Property(e => e.UserType)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("User_Type");

            entity.HasOne(d => d.Co).WithMany(p => p.Users)
                .HasForeignKey(d => d.CoId)
                .HasConstraintName("FK_user_company");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
