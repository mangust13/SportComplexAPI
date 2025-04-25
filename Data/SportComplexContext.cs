// Data/SportComplexContext.cs
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

        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<Client> Clients { get; set; } = null!;
        public DbSet<Gender> Genders { get; set; } = null!;
        public DbSet<PaymentMethod> PaymentMethods { get; set; }

        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<BaseSubscription> BaseSubscriptions { get; set; }

        public DbSet<SubscriptionTerm> SubscriptionTerms { get; set; }
        public DbSet<SubscriptionVisitTime> SubscriptionVisitTimes { get; set; }

        public DbSet<SubscriptionActivity> SubscriptionActivities { get; set; }
        public DbSet<Activity> Activities { get; set; }
    }
}