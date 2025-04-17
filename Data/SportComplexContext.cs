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

        public DbSet<Client> Clients { get; set; } = null!;
        public DbSet<Gender> Genders { get; set; } = null!;
    }
}