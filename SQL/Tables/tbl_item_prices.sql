CREATE TABLE `tbl_item_prices` (
  `ItemsId` int NOT NULL,
  `ItemsSubId` int NOT NULL,
  `ItemUuid` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CostPrice` decimal(18,2) NOT NULL,
  `SellingPrice` decimal(18,2) NOT NULL,
  `WholesalePrice` decimal(18,2) DEFAULT NULL,
  PRIMARY KEY (`ItemsId`,`ItemsSubId`),
  UNIQUE KEY `AK_tbl_item_prices_ItemUuid` (`ItemUuid`),
  CONSTRAINT `FK_tbl_item_prices_tbl_items_ItemsId_ItemsSubId` FOREIGN KEY (`ItemsId`, `ItemsSubId`) REFERENCES `tbl_items` (`Id`, `SubId`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
