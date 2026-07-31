CREATE TABLE `tbl_item_expiries` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ItemsId` int NOT NULL,
  `ItemsSubId` int NOT NULL,
  `ItemUuid` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ExpiryDate` datetime(6) NOT NULL,
  `NotifyBeforeDays` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_tbl_item_expiries_ItemsId_ItemsSubId` (`ItemsId`,`ItemsSubId`),
  CONSTRAINT `FK_tbl_item_expiries_tbl_items_ItemsId_ItemsSubId` FOREIGN KEY (`ItemsId`, `ItemsSubId`) REFERENCES `tbl_items` (`Id`, `SubId`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
