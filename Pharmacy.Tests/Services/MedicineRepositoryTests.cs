using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.EntityFrameworkCore;
using Pharmacy.API.Data;
using Pharmacy.API.Models;
using Pharmacy.API.Services;
using Microsoft.Data.Sqlite;


namespace Pharmacy.Tests.Services
{
    public class MedicineRepositoryTests
    {
        private PharmacyDbContext GetDbContext()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var options = new DbContextOptionsBuilder<PharmacyDbContext>()
                .UseSqlite(connection).Options;
            var context = new PharmacyDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public async Task UpdateAsync_ModifiesDatabaseRecord()
        {
            // Arrange
            using var context = GetDbContext();
            var repo = new MedicineRepository(context);
            var med = new Medicine { Name = "Paracetamol", StockQuantity = 50 };
            context.Medicines.Add(med);
            await context.SaveChangesAsync();

            // Act
            med.StockQuantity = 40;
            await repo.UpdateAsync(med);

            // Assert
            var updated = await context.Medicines.FindAsync(med.MedicineId);
            Assert.Equal(40, updated?.StockQuantity);
        }
    }
}
