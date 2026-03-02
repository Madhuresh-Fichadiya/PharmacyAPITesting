using Pharmacy.API.Data;
using Pharmacy.API.Models;

namespace Pharmacy.API.Services
{
    public interface ITransactionRepository
    {
        public Task AddAsync(StockTransaction stockTransaction);
    }

    public class TransactionRepository : ITransactionRepository
    {
        private readonly PharmacyDbContext _context;

        public TransactionRepository(PharmacyDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(StockTransaction transaction)
        {
            // Adds the new transaction record to the tracking list
            await _context.Transactions.AddAsync(transaction);

            // Persists the change to the database
            await _context.SaveChangesAsync();
        }
    }
}