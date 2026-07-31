CREATE TABLE `tbl_item_suppliers` (
  `SuppliersId` int NOT NULL,
  `ItemsId` int NOT NULL,
  `ItemsSubId` int NOT NULL,
  `Uuid` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`SuppliersId`,`ItemsId`,`ItemsSubId`),
  UNIQUE KEY `AK_tbl_item_suppliers_Uuid` (`Uuid`),
  KEY `IX_tbl_item_suppliers_ItemsId_ItemsSubId` (`ItemsId`,`ItemsSubId`),
  CONSTRAINT `FK_tbl_item_suppliers_tbl_items_ItemsId_ItemsSubId` FOREIGN KEY (`ItemsId`, `ItemsSubId`) REFERENCES `tbl_items` (`Id`, `SubId`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_tbl_item_suppliers_tbl_suppliers_SuppliersId` FOREIGN KEY (`SuppliersId`) REFERENCES `tbl_suppliers` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
