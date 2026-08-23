using pos_service.Models;
using pos_service.Models.DTO.Purchases;

namespace pos_service.Services.Purchases
{
    /// <summary>
    /// Contract defining purchase receipt management and batch stock receipt operations.
    /// </summary>
    public interface IPurchaseService
    {
        /// <summary>
        /// Retrieves all purchase receipts recorded in the system.
        /// </summary>
        /// <param name="currentUser">The authenticated user executing the query.</param>
        /// <returns>A collection of purchase receipt response DTOs.</returns>
        Task<IEnumerable<PurchaseResDto>> GetAllPurchasesAsync(CurrentUser? currentUser = null);

        /// <summary>
        /// Retrieves a specific purchase receipt by its unique UUID.
        /// </summary>
        /// <param name="purchaseUuid">The unique UUID identifier of the purchase receipt.</param>
        /// <param name="currentUser">The authenticated user executing the query.</param>
        /// <returns>The purchase receipt response DTO if found, otherwise null.</returns>
        Task<PurchaseResDto?> GetByUuidAsync(string purchaseUuid, CurrentUser? currentUser = null);

        /// <summary>
        /// Creates a new purchase receipt, generates stock batches, logs initial stock movements, and updates item expiries.
        /// </summary>
        /// <param name="dto">The purchase receipt creation payload containing supplier, invoice, and line items.</param>
        /// <param name="currentUser">The authenticated user creating the purchase receipt.</param>
        /// <returns>The created purchase receipt response DTO.</returns>
        Task<PurchaseResDto> CreatePurchaseAsync(PurchaseReqDto dto, CurrentUser? currentUser = null);

        /// <summary>
        /// Deletes a purchase receipt by its unique UUID.
        /// </summary>
        /// <param name="purchaseUuid">The unique UUID identifier of the purchase receipt to delete.</param>
        /// <param name="currentUser">The authenticated user performing the deletion.</param>
        /// <returns>True if the purchase receipt was successfully deleted, otherwise false.</returns>
        Task<bool> DeletePurchaseAsync(string purchaseUuid, CurrentUser? currentUser = null);
    }
}
