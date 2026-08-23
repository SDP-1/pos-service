using pos_service.Models;

namespace pos_service.Repositories.Purchases
{
    public interface IPurchaseRepository
    {
        /// <summary>
        /// Retrieves all purchase records with supplier details and batch collections.
        /// </summary>
        /// <returns>Collection of Purchase entities.</returns>
        Task<IEnumerable<Purchase>> GetAllPurchasesAsync();

        /// <summary>
        /// Retrieves a purchase record by its unique identifier (UUID), including supplier and batch details.
        /// </summary>
        /// <param name="purchaseUuid">The unique identifier (UUID) of the purchase.</param>
        /// <returns>Purchase entity when found; otherwise null.</returns>
        Task<Purchase?> GetByUuidAsync(string purchaseUuid);

        /// <summary>
        /// Adds a new purchase record to the database and saves changes.
        /// </summary>
        /// <param name="purchase">The purchase entity to insert.</param>
        /// <returns>The created Purchase entity.</returns>
        Task<Purchase> AddPurchaseAsync(Purchase purchase);

        /// <summary>
        /// Updates an existing purchase record in the database.
        /// </summary>
        /// <param name="purchase">The purchase entity with modified values.</param>
        /// <returns>The updated Purchase entity.</returns>
        Task<Purchase> UpdatePurchaseAsync(Purchase purchase);

        /// <summary>
        /// Soft-deletes a purchase record by marking its IsActive flag to false.
        /// </summary>
        /// <param name="purchaseUuid">The unique identifier (UUID) of the purchase to delete.</param>
        /// <returns>True if the purchase was found and deleted; otherwise false.</returns>
        Task<bool> DeletePurchaseAsync(string purchaseUuid);

        /// <summary>
        /// Generates a sequential, user-friendly purchase order number for the current day (e.g., PO-YYYYMMDD-XXXX).
        /// </summary>
        /// <returns>A formatted unique purchase number string.</returns>
        Task<string> GeneratePurchaseNumberAsync();
    }
}
