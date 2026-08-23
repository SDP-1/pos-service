CREATE TABLE IF NOT EXISTS `tbl_item_units` (
  `Id`                  int NOT NULL AUTO_INCREMENT,
  `ItemUuid`            varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `UnitType`            varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ParentUnitType`      varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `QuantityPerParent`   decimal(18,3) DEFAULT NULL,
  `QuantityInBaseUnits` decimal(18,3) NOT NULL,
  `IsBaseUnit`          tinyint(1) NOT NULL DEFAULT '0',
  `Uuid`                varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `AK_tbl_item_units_Uuid` (`Uuid`),
  KEY `IX_tbl_item_units_ItemUuid` (`ItemUuid`),
  CONSTRAINT `FK_tbl_item_units_tbl_items_ItemUuid` FOREIGN KEY (`ItemUuid`) REFERENCES `tbl_items` (`Uuid`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
