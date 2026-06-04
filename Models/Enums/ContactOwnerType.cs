namespace pos_service.Models.Enums
{
    /// <summary>
    /// Specifies the type of entity that owns a contact.
    /// Used to determine which foreign key relationship to use.
    /// </summary>
    public enum ContactOwnerType
    {
        /// <summary>
        /// Contact is associated with a User entity.
        /// </summary>
        User     = 1,

        /// <summary>
        /// Contact is associated with a Supplier entity.
        /// </summary>
        Supplier = 2
    }
}
