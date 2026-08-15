using pos_service.Models;

namespace pos_service.Repositories
{
    public interface IUserRepository
    {
        /// <summary>
        /// Retrieves all users from the data store.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// a sequence of all <see cref="User"/> entities.
        /// </returns>
        Task<IEnumerable<User>> GetAllAsync();

        /// <summary>
        /// Retrieves a specific user by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// the matching <see cref="User"/> if found; otherwise, <see langword="null"/>.
        /// </returns>
        Task<User?> GetByIdAsync(int id);

        /// <summary>
        /// Retrieves a user by username for authentication and identity lookup purposes.
        /// </summary>
        /// <param name="userName">The username to search for.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// the matching <see cref="User"/> if found; otherwise, <see langword="null"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="userName"/> is null, empty, or whitespace.
        /// </exception>
        Task<User?> GetByUserNameAsync(string userName);

        /// <summary>
        /// Retrieves a user by its unique identifier, including related contact information.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// the matching <see cref="User"/> with contact information if found; otherwise, <see langword="null"/>.
        /// </returns>
        Task<User?> GetByIdWithContactsAsync(int id);

        /// <summary>
        /// Retrieves a user by UUID.
        /// </summary>
        /// <param name="uuid">The UUID identifier of the user.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// the matching <see cref="User"/> if found; otherwise, <see langword="null"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="uuid"/> is null, empty, or whitespace.
        /// </exception>
        Task<User?> GetByUuidAsync(string uuid);

        /// <summary>
        /// Adds a new user and contacts inside a repository transaction.
        /// </summary>
        Task<User> SaveNewUserWithContactsAsync(User user, IEnumerable<Contact>? contacts);

        /// <summary>
        /// Adds a new user to the data store.
        /// </summary>
        /// <param name="user">The user entity to add.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// the added <see cref="User"/> entity with generated identifiers and persisted values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="user"/> is <see langword="null"/>.
        /// </exception>
        Task<User> AddAsync(User user);

        /// <summary>
        /// Updates an existing user in the data store.
        /// </summary>
        /// <param name="user">The user entity with updated information.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// the updated <see cref="User"/> entity.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="user"/> is <see langword="null"/>.
        /// </exception>
        Task<User> UpdateAsync(User user);

        /// <summary>
        /// Deletes a user from the data store.
        /// </summary>
        /// <param name="user">The user entity to delete.</param>
        /// <returns>A task that represents the asynchronous delete operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="user"/> is <see langword="null"/>.
        /// </exception>
        Task DeleteAsync(User user);

        /// <summary>
        /// Checks if a user with the specified identifier exists.
        /// </summary>
        /// <param name="id">The unique identifier to check.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result is
        /// <see langword="true"/> if the user exists; otherwise, <see langword="false"/>.
        /// </returns>
        Task<bool> UserExistsAsync(int id);
    }
}