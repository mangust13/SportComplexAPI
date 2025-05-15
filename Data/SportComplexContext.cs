using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using SportComplexAPI.Models;

namespace SportComplexAPI.Data
{
    public class SportComplexContext : DbContext
    {
        public SportComplexContext(DbContextOptions<SportComplexContext> options)
            : base(options) { }


        //Authorization
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UserRole> UserRoles { get; set; } = null!;
        //Classifiers 10
        public DbSet<Gender> Genders { get; set; } = null!;
        public DbSet<GymStatus> GymStatuses { get; set; } = null!;
        public DbSet<Models.DayOfWeek> DaysOfWeek { get; set; } = null!;
        public DbSet<PaymentMethod> PaymentMethods { get; set; } = null!;
        public DbSet<City> Cities { get; set; } = null!;
        public DbSet<SubscriptionTerm> SubscriptionTerms { get; set; } = null!;
        public DbSet<SubscriptionVisitTime> SubscriptionVisitTimes { get; set; } = null!;
        public DbSet<OrderStatus> OrderStatuses { get; set; } = null!;
        public DbSet<Brand> Brands { get; set; } = null!;
        public DbSet<ProductType> ProductTypes { get; set; } = null!;

        //Objects 11
        public DbSet<SportComplex> SportComplexes { get; set; } = null!;
        public DbSet<Gym> Gyms { get; set; } = null!;
        public DbSet<Trainer> Trainers { get; set; } = null!;
        public DbSet<Client> Clients { get; set; } = null!;
        public DbSet<BaseSubscription> BaseSubscriptions { get; set; } = null!;
        public DbSet<Subscription> Subscriptions { get; set; } = null!;
        public DbSet<Activity> Activities { get; set; } = null!;
        public DbSet<Schedule> Schedules { get; set; } = null!;
        public DbSet<ActivityInGym> ActivityInGyms { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Supplier> Suppliers { get; set; } = null!;

        //Intermediate 2
        public DbSet<SubscriptionActivity> SubscriptionActivities { get; set; } = null!;
        public DbSet<TrainerActivity> TrainerActivities { get; set; } = null!;

        // Documents 8
        public DbSet<Purchase> Purchases { get; set; } = null!;
        public DbSet<TrainerSchedule> TrainerSchedules { get; set; } = null!;
        public DbSet<Training> Trainings { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<PurchasedProduct> PurchasedProducts {  get; set; } = null!;
        public DbSet<Inventory> Inventory { get; set; } = null!;
        public DbSet<Delivery> Deliveries { get; set; } = null!;
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<SubscriptionActivity>()
                .ToTable(tb => tb.HasTrigger("trg_UpdateSubscriptionTotal"));

            modelBuilder.Entity<Delivery>()
                .ToTable(tb => tb.HasTrigger("trg_UpdateOrderStatus"));

            modelBuilder.Entity<Delivery>()
                .ToTable(tb => tb.HasTrigger("trg_UpdateInventoryQuantity"));
        }

    }
}