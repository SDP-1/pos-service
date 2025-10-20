using pos_service.Models.DTO;

namespace pos_service.Services
{
    public interface ISupplierService
    {
        Task<SupplierResDto?> GetSupplierByIdAsync(int id);
        Task<IEnumerable<SupplierResDto>> GetAllSuppliersAsync();
        Task<SupplierResDto> CreateSupplierAsync(SupplierReqDto dto);
        Task<bool> UpdateSupplierAsync(int id, SupplierReqDto dto);
        Task<bool> DeleteSupplierAsync(int id);
    }
}
