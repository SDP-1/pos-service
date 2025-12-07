using pos_service.Models;

namespace pos_service.Repositories
{
    public interface IUserRepository
    {
        /// <summary>
        /// Retrieves all users from the data store.
        /// </summary>
        /// <returns>A list of all user entities.</returns>
        Task<IEnumerable<User>> GetAllAsync();

        /// <summary>
        /// Retrieves a specific user by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <returns>The user entity if found, otherwise null.</returns>
        Task<User?> GetByIdAsync(int id);

        /// <summary>
        /// Retrieves a user by their username for authentication purposes.
        /// </summary>
        /// <param name="userName">The username to search for.</param>
        /// <returns>The user entity if found, otherwise null.</returns>
        Task<User?> GetByUserNameAsync(string userName);

        /// <summary>
        /// Retrieves a user by its unique identifier including related contact information.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <returns>The user entity with contact information if found, otherwise null.</returns>
        Task<User?> GetByIdWithContactsAsync(int id);

        /// <summary>
        /// Retrieves a user by its unique identifier including related contact information.
        /// </summary>
        /// <param name="uuid">The Uuid identifier of the user.</param>
        /// <returns>The user entity with contact information if found, otherwise null.</returns>
        Task<User?> GetByUuidAsync(string uuid);

        /// <summary>
        /// Adds a new user to the data store.
        /// </summary>
        /// <param name="user">The user entity to add.</param>
        /// <returns>The added user entity with updated identifiers.</returns>
        Task<User> AddAsync(User user);

        /// <summary>
        /// Updates an existing user in the data store.
        /// </summary>
        /// <param name="user">The user entity with updated information.</param>
        /// <returns>The updated user entity.</returns>
        Task<User> UpdateAsync(User user);

        /// <summary>
        /// Deletes a user from the data store.
        /// </summary>
        /// <param name="user">The user entity to delete.</param>
        Task DeleteAsync(User user);

        /// <summary>
        /// Checks if a user with the specified identifier exists.
        /// </summary>
        /// <param name="id">The unique identifier to check.</param>
        /// <returns>True if the user exists, otherwise false.</returns>
        Task<bool> UserExistsAsync(int id);
    }
}