
using Pharmacy.API.Data;
using Pharmacy.API.Models;

namespace Pharmacy.API.Services
{
    public interface IMedicineRepository
    {
        public Task<Medicine> GetByIdAsync(int medicineId);
        Task UpdateAsync(Medicine medicine);
    }

    public class MedicineRepository : IMedicineRepository
    {
        private readonly PharmacyDbContext _context;

        public MedicineRepository(PharmacyDbContext context)
        {
            _context = context;
        }

        public async Task<Medicine> GetByIdAsync(int id)
        {
            // Finds the medicine record or returns null if not found
            return await _context.Medicines.FindAsync(id);
        }

        public async Task UpdateAsync(Medicine medicine)
        {
            // Marks the entity as modified so EF knows to generate an UPDATE statement
            _context.Medicines.Update(medicine);
            await _context.SaveChangesAsync();
        }
    }
}