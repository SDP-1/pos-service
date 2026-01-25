using pos_service.Models;
using pos_service.Models.DTO.Suppliers;

namespace pos_service.Services
{
    public interface ISupplierService
    {
        /// <summary>
        /// Retrieves a specific supplier by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the supplier.</param>
        /// <param name="currentUser">The current user requesting the supplier.</param>
        /// <returns>The supplier details if found, otherwise null.</returns>
        Task<SupplierResDto?> GetSupplierByIdAsync(int id, CurrentUser currentUser);

        /// <summary>
        /// Retrieves all suppliers from the system.
        /// </summary>
        /// <param name="currentUser">The current user requesting the suppliers.</param>
        /// <returns>A list of all supplier details.</returns>
        Task<IEnumerable<SupplierResDto>> GetAllSuppliersAsync(CurrentUser currentUser);

        /// <summary>
        /// Creates a new supplier in the system.
        /// </summary>
        /// <param name="dto">The supplier data transfer object containing supplier information.</param>
        /// <param name="currentUser">The current user creating the supplier.</param>
        /// <returns>The newly created supplier details.</returns>
        Task<SupplierResDto> CreateSupplierAsync(SupplierReqDto dto, CurrentUser currentUser);

        /// <summary>
        /// Updates an existing supplier with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the supplier to update.</param>
        /// <param name="dto">The supplier data transfer object containing updated information.</param>
        /// <param name="currentUser">The current user updating the supplier.</param>
        /// <returns>True if update was successful, otherwise false.</returns>
        Task<bool> UpdateSupplierAsync(int id, SupplierReqDto dto, CurrentUser currentUser);

        /// <summary>
        /// Deletes a supplier with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the supplier to delete.</param>
        /// <param name="currentUser">The current user deleting the supplier.</param>
        /// <returns>True if deletion was successful, otherwise false.</returns>
        Task<bool> DeleteSupplierAsync(int id, CurrentUser currentUser);

        /// <summary>
        /// Retrieves a specific supplier with its items by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the supplier.</param>
        /// <param name="currentUser">The current user requesting the supplier.</param>
        /// <returns>The supplier details with items if found, otherwise null.</returns>
        Task<SupplierResDto?> GetSupplierWithItemsAsync(int id, CurrentUser currentUser);


        /// <summary>
        /// Retrieves lightweight supplier data for dropdowns (Id and Name only).
        /// Returns minimal DTO to avoid mapping large SupplierResDto objects.
        /// </summary>
        Task<IEnumerable<SupplierDropdownDto>> GetSuppliersDropdownAsync(CurrentUser currentUser);
    }
}