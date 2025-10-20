using pos_service.Models;

namespace pos_service.Repositories
{
    public interface ISupplierRepository
    {
        Task<Supplier?> GetByIdAsync(int id);
        Task<Supplier?> GetByIdWithDetailsAsync(int id); // To load related data
        Task<IEnumerable<Supplier>> GetAllAsync();
        Task<Supplier> AddAsync(Supplier supplier);
        Task<Supplier> UpdateAsync(Supplier supplier);
        Task DeleteAsync(Supplier supplier);
    }
}
