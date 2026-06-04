namespace pos_service.Models.Enums
{
    /// <summary>
    /// Unit types used to describe product packaging and measurement units.
    /// </summary>
    public enum UnitType
    {
        /// <summary>
        /// No unit specified.
        /// </summary>
        None       = 0,

        /// <summary>
        /// Each (individual item).
        /// </summary>
        Each       = 1,

        /// <summary>
        /// Piece (single piece).
        /// </summary>
        Piece      = 2,

        /// <summary>
        /// Packet packaging.
        /// </summary>
        Packet     = 3,

        /// <summary>
        /// Pack packaging.
        /// </summary>
        Pack       = 4,

        /// <summary>
        /// Box packaging.
        /// </summary>
        Box        = 5,

        /// <summary>
        /// Carton packaging.
        /// </summary>
        Carton     = 6,

        /// <summary>
        /// Bundle packaging.
        /// </summary>
        Bundle     = 7,

        /// <summary>
        /// Set of items sold together.
        /// </summary>
        Set        = 8,

        /// <summary>
        /// Bottle packaging.
        /// </summary>
        Bottle     = 9,

        /// <summary>
        /// Can packaging.
        /// </summary>
        Can        = 10,

        /// <summary>
        /// Jar packaging.
        /// </summary>
        Jar        = 11,

        /// <summary>
        /// Tube packaging.
        /// </summary>
        Tube       = 12,

        /// <summary>
        /// Kilogram weight unit.
        /// </summary>
        Kilogram   = 13,

        /// <summary>
        /// Gram weight unit.
        /// </summary>
        Gram       = 14,

        /// <summary>
        /// Pound weight unit.
        /// </summary>
        Pound      = 15,

        /// <summary
        /// Liter volume unit.
        /// </summary>
        Liter      = 16,

        /// <summary>
        /// Milliliter volume unit.
        /// </summary>
        Milliliter = 17,

        /// <summary>
        /// Gallon volume unit.
        /// </summary>
        Gallon     = 18
    }
}
