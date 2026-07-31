CREATE TABLE `tbl_inventory_units` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `InventoryId` int NOT NULL,
  `UnitType` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ParentUnitType` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `QuantityPerParent` decimal(18,3) DEFAULT NULL,
  `QuantityInBaseUnits` decimal(18,3) NOT NULL,
  `IsBaseUnit` tinyint(1) NOT NULL,
  `Uuid` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `AK_tbl_inventory_units_Uuid` (`Uuid`),
  KEY `IX_tbl_inventory_units_InventoryId` (`InventoryId`),
  CONSTRAINT `FK_tbl_inventory_units_tbl_inventories_InventoryId` FOREIGN KEY (`InventoryId`) REFERENCES `tbl_inventories` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
