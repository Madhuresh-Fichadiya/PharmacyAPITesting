using Microsoft.EntityFrameworkCore;
using Pharmacy.API.Models;
using System.Collections.Generic;

namespace Pharmacy.API.Data
{
    public class PharmacyDbContext : DbContext
    {
        public PharmacyDbContext(DbContextOptions<PharmacyDbContext> options) : base(options) 
        { 
        }

        public DbSet<Medicine> Medicines { get; set; }
        public DbSet<StockTransaction> Transactions { get; set; }
    }
}
