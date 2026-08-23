CREATE TABLE IF NOT EXISTS `tbl_item_price_audits` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `ItemsId` int NOT NULL,
  `ItemsSubId` int NOT NULL,
  `ItemUuid` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `BuyingPrice` decimal(18,2) NOT NULL,
  `MarkedPrice` decimal(18,2) NOT NULL,
  `RetailPrice` decimal(18,2) NOT NULL,
  `WholesalePrice` decimal(18,2) NOT NULL,
  `RetailDiscountRatio` decimal(5,2) NOT NULL DEFAULT '0.00',
  `WholesaleDiscountRatio` decimal(5,2) NOT NULL DEFAULT '0.00',
  `ActionDate` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ActionBy` varchar(255) DEFAULT NULL,
  `Action` varchar(10) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_tbl_item_price_audits_ItemUuid_ActionDate` (`ItemUuid`,`ActionDate`),
  KEY `IX_tbl_item_price_audits_ItemsId_ItemsSubId` (`ItemsId`,`ItemsSubId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
