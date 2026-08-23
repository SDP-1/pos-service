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

        /// <summary>
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
        Gallon     = 18,

        /// <summary>
        /// Sack packaging (e.g., bulk rice, flour, grain, sugar).
        /// </summary>
        Sack       = 19,

        /// <summary>
        /// Bag packaging (e.g., shopping bag, produce bag, cement bag).
        /// </summary>
        Bag        = 20,

        /// <summary>
        /// Crate packaging (e.g., beverage crate, fruit crate).
        /// </summary>
        Crate      = 21,

        /// <summary>
        /// Pallet packaging (bulk shipping/warehouse platform).
        /// </summary>
        Pallet     = 22,

        /// <summary>
        /// Tin packaging (e.g., biscuits, oil, paint, milk powder).
        /// </summary>
        Tin        = 23,

        /// <summary>
        /// Tub packaging (e.g., ice cream, butter, paste).
        /// </summary>
        Tub        = 24,

        /// <summary>
        /// Roll packaging (e.g., paper roll, tape, fabric, wire).
        /// </summary>
        Roll       = 25,

        /// <summary>
        /// Dozen quantity unit (group of 12 items).
        /// </summary>
        Dozen      = 26,

        /// <summary>
        /// Tray packaging (e.g., egg tray, bakery tray, meat tray).
        /// </summary>
        Tray       = 27,

        /// <summary>
        /// Meter length unit.
        /// </summary>
        Meter      = 28,

        /// <summary>
        /// Centimeter length unit.
        /// </summary>
        Centimeter = 29,

        /// <summary>
        /// Foot length unit.
        /// </summary>
        Foot       = 30,

        /// <summary>
        /// Inch length unit.
        /// </summary>
        Inch       = 31,

        /// <summary>
        /// Ounce weight unit.
        /// </summary>
        Ounce      = 32,

        /// <summary>
        /// Bar packaging (e.g., soap bar, chocolate bar).
        /// </summary>
        Bar        = 33,

        /// <summary>
        /// Sachet packaging (e.g., shampoo, coffee, seasoning single-use packets).
        /// </summary>
        Sachet     = 34,

        /// <summary>
        /// Pair unit (group of 2 matching items).
        /// </summary>
        Pair       = 35,

        /// <summary>
        /// Bale packaging (e.g., compressed textile, paper, hay).
        /// </summary>
        Bale       = 36,

        /// <summary>
        /// Case packaging (beverage case, shipping case).
        /// </summary>
        Case       = 37
    }
}
