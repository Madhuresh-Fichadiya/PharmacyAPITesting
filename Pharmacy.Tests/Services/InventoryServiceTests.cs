using Moq;
using Pharmacy.API.Models;
using Pharmacy.API.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Tests.Services
{
    public class InventoryServiceTests
    {
        private readonly Mock<IMedicineRepository> _medRepoMock;
        private readonly Mock<ITransactionRepository> _transRepoMock;
        private readonly InventoryService _service;

        public InventoryServiceTests()
        {
            _medRepoMock = new Mock<IMedicineRepository>();
            _transRepoMock = new Mock<ITransactionRepository>();
            _service = new InventoryService(_medRepoMock.Object, _transRepoMock.Object);
        }

        // 1. SCENARIO: Medicine does not exist in database
        [Fact]
        public async Task ProcessSale_MedicineNotFound_ThrowsKeyNotFoundException()
        {
            _medRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Medicine)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.ProcessSaleAsync(999, 1));
        }

        // 2. SCENARIO: Medicine is expired
        [Fact]
        public async Task ProcessSale_ExpiredMedicine_ThrowsInvalidOperationException()
        {
            var expiredMed = new Medicine { MedicineId = 1, ExpiryDate = DateTime.UtcNow.AddMinutes(-5) };
            _medRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(expiredMed);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ProcessSaleAsync(1, 1));
        }

        // 3. SCENARIO: Testing boundary conditions for stock
        [Theory]
        [InlineData(10, 15, false)] // Requesting more than available (Fail)
        [InlineData(10, 10, true)]  // Requesting exactly what is available (Boundary - Pass)
        [InlineData(10, 1, true)]   // Requesting less than available (Pass)
        [InlineData(10, 0, true)]   // Requesting zero (Edge case - Pass/Logic dependent)
        public async Task ProcessSale_StockBoundaries_ReturnsExpected(int currentStock, int requested, bool expectedResult)
        {
            var med = new Medicine { MedicineId = 1, StockQuantity = currentStock, ExpiryDate = DateTime.UtcNow.AddDays(1) };
            _medRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(med);

            var result = await _service.ProcessSaleAsync(1, requested);

            Assert.Equal(expectedResult, result);
        }

        // 4. SCENARIO: Verify that database updates actually occur on success
        [Fact]
        public async Task ProcessSale_OnSuccess_VerifiesRepositoryCalls()
        {
            // Arrange
            var med = new Medicine { MedicineId = 1, StockQuantity = 10, ExpiryDate = DateTime.UtcNow.AddDays(1) };
            _medRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(med);

            // Act
            await _service.ProcessSaleAsync(1, 3);

            // Assert: Verify that UpdateAsync was called with the new stock value (7)
            _medRepoMock.Verify(r => r.UpdateAsync(It.Is<Medicine>(m => m.StockQuantity == 7)), Times.Once);

            // Assert: Verify that a transaction record was created
            _transRepoMock.Verify(r => r.AddAsync(It.Is<StockTransaction>(t => t.QuantityChange == -3)), Times.Once);
        }
    }
}
