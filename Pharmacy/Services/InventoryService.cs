using Pharmacy.API.Models;

namespace Pharmacy.API.Services
{
    public interface IInventoryService
    {
        Task<bool> ProcessSaleAsync(int medicineId, int quantity);
    }
    public class InventoryService : IInventoryService
    {
        private readonly IMedicineRepository _medicineRepo;
        private readonly ITransactionRepository _transactionRepo;

        public InventoryService(IMedicineRepository medicineRepo, ITransactionRepository transactionRepo)
        {
            _medicineRepo = medicineRepo;
            _transactionRepo = transactionRepo;
        }

        public async Task<bool> ProcessSaleAsync(int medicineId, int quantity)
        {
            var medicine = await _medicineRepo.GetByIdAsync(medicineId);

            // Rule 1: Existence check
            if (medicine == null) throw new KeyNotFoundException("Medicine not found.");

            // Rule 2: Expiration check
            if (medicine.ExpiryDate <= DateTime.UtcNow) throw new InvalidOperationException("Cannot sell expired medicine.");

            // Rule 3: Stock availability
            if (medicine.StockQuantity < quantity) return false;

            // Process the update
            medicine.StockQuantity -= quantity;
            await _medicineRepo.UpdateAsync(medicine);
            await _transactionRepo.AddAsync(new StockTransaction
            {
                MedicineId = medicineId,
                QuantityChange = -quantity,
                TransactionDate = DateTime.UtcNow
            });

            return true;
        }
    }
}
