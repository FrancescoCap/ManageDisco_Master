using ManageDisco.Model;
using ManageDisco.Model.UserIdentity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Context
{
    public class DiscoContext : IdentityDbContext<User>
    {
        public static readonly LoggerFactory loggerFactory = new LoggerFactory(new[] { new Microsoft.Extensions.Logging.Debug.DebugLoggerProvider() });

        public DiscoContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<IdentityRole> Roles { get; set; }
        public DbSet<EventParty> Events { get; set; }
        public DbSet<ReservationType> ReservationType { get; set; }
        public DbSet<Reservation> Reservation { get; set; }
        public DbSet<ReservationUserCode> ReservationUserCode { get; set; }
        public DbSet<ReservationStatus> ReservationStatus { get; set; }
        public DbSet<ReservationPayment> ReservationPayment { get; set; }
        public DbSet<PaymentOverview> PaymentOverview { get; set; }
        public DbSet<DiscoEntity> DiscoEntity  { get; set; }
        public DbSet<Table> Table { get; set; }
        public DbSet<Catalog> Catalog { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<TableOrderHeader> TableOrderHeader { get; set; }
        public DbSet<TableOrderRow> TableOrderRow { get; set; }
        public DbSet<EventStatus> EventStatus { get; set; }
        public DbSet<PrCustomer> PrCustomer { get; set; }
        public DbSet<EventPhoto> EventPhoto { get; set; }
        public DbSet<PhotoType> PhotoType { get; set; }
        public DbSet<HomePhoto> HomePhoto { get; set; }
        public DbSet<Warehouse> Warehouse { get; set; }
        public DbSet<Contact> Contact { get; set; }
        public DbSet<ContactType> ContactType { get; set; }
        public DbSet<Coupon> Coupon { get; set; }
        public DbSet<ProductShopHeader> ProductShopHeader { get; set; }
        public DbSet<UserProduct> UserProduct { get; set; }
        public DbSet<ProductShopType> ProductShopType { get; set; }
        public DbSet<ProductShopRow> ProductShopRow { get; set; }
        public DbSet<RefreshToken> RefreshToken { get; set; }
        public DbSet<PermissionAction> PermissionAction { get; set; }
        public DbSet<UserPermission> UserPermission { get; set; }
        public DbSet<AnonymusAllowed> AnonymusAllowed { get; set; }
        public DbSet<TableCouponUsed> TableCouponUsed { get; set; }
        public DbSet<TablePreOrderHeader> TablePreOrderHeader { get; set; }
        public DbSet<TablePreOrderRow> TablePreOrderRow { get; set; }
        public DbSet<Cookie> Cookies { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLoggerFactory(loggerFactory);
            base.OnConfiguring(optionsBuilder);
        }
    }
}
