using System.ComponentModel.DataAnnotations;

namespace Pharmacy.API.Models
{
    public class Medicine
    {
        [Key]
        public int MedicineId { get; set; }
        public string Name { get; set; }
        public int StockQuantity { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool RequiresPrescription { get; set; }
    }

    public class StockTransaction
    {
        [Key]
        public int StockTransactionId { get; set; }
        public int MedicineId { get; set; }
        public int QuantityChange { get; set; } // Positive for restock, negative for sales
        public DateTime TransactionDate { get; set; }
    }
}
