-- ====================================================================================
-- TABLE SCHEMA: tbl_item_prices
-- INCLUDES TRIGGERS: trg_tbl_item_prices_after_insert, trg_tbl_item_prices_after_update
-- ====================================================================================

CREATE TABLE IF NOT EXISTS `tbl_item_prices` (
  `ItemsId` int NOT NULL,
  `ItemsSubId` int NOT NULL,
  `ItemUuid` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `BuyingPrice` decimal(18,2) NOT NULL DEFAULT '0.00',
  `MarkedPrice` decimal(18,2) NOT NULL DEFAULT '0.00',
  `RetailPrice` decimal(18,2) NOT NULL DEFAULT '0.00',
  `WholesalePrice` decimal(18,2) NOT NULL DEFAULT '0.00',
  `RetailDiscountRatio` decimal(5,2) NOT NULL DEFAULT '0.00',
  `WholesaleDiscountRatio` decimal(5,2) NOT NULL DEFAULT '0.00',
  `Uuid` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  `CreatedBy` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `UpdatedBy` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`ItemsId`,`ItemsSubId`),
  UNIQUE KEY `AK_tbl_item_prices_ItemUuid` (`ItemUuid`),
  UNIQUE KEY `AK_tbl_item_prices_Uuid` (`Uuid`),
  CONSTRAINT `FK_tbl_item_prices_tbl_items_ItemsId_ItemsSubId` FOREIGN KEY (`ItemsId`, `ItemsSubId`) REFERENCES `tbl_items` (`Id`, `SubId`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;


-- ====================================================================================
-- TRIGGERS ON tbl_item_prices
-- ====================================================================================

-- Trigger: After Insert on tbl_item_prices
DROP TRIGGER IF EXISTS `trg_tbl_item_prices_after_insert`;
DELIMITER $$
CREATE TRIGGER `trg_tbl_item_prices_after_insert`
AFTER INSERT ON `tbl_item_prices`
FOR EACH ROW
BEGIN
  INSERT INTO `tbl_item_price_audits` (
    `ItemsId`, `ItemsSubId`, `ItemUuid`, `BuyingPrice`, `MarkedPrice`, `RetailPrice`, `WholesalePrice`, `RetailDiscountRatio`, `WholesaleDiscountRatio`, `ActionDate`, `ActionBy`, `Action`
  ) VALUES (
    NEW.`ItemsId`, NEW.`ItemsSubId`, NEW.`ItemUuid`, NEW.`BuyingPrice`, NEW.`MarkedPrice`, NEW.`RetailPrice`, NEW.`WholesalePrice`, NEW.`RetailDiscountRatio`, NEW.`WholesaleDiscountRatio`, NOW(), NEW.`CreatedBy`, 'INSERT'
  );
END$$
DELIMITER ;

-- Trigger: After Update on tbl_item_prices
DROP TRIGGER IF EXISTS `trg_tbl_item_prices_after_update`;
DELIMITER $$
CREATE TRIGGER `trg_tbl_item_prices_after_update`
AFTER UPDATE ON `tbl_item_prices`
FOR EACH ROW
BEGIN
  IF (OLD.`BuyingPrice` <> NEW.`BuyingPrice` OR
      OLD.`MarkedPrice` <> NEW.`MarkedPrice` OR
      OLD.`RetailPrice` <> NEW.`RetailPrice` OR
      OLD.`WholesalePrice` <> NEW.`WholesalePrice` OR
      OLD.`RetailDiscountRatio` <> NEW.`RetailDiscountRatio` OR
      OLD.`WholesaleDiscountRatio` <> NEW.`WholesaleDiscountRatio`) THEN
    INSERT INTO `tbl_item_price_audits` (
      `ItemsId`, `ItemsSubId`, `ItemUuid`, `BuyingPrice`, `MarkedPrice`, `RetailPrice`, `WholesalePrice`, `RetailDiscountRatio`, `WholesaleDiscountRatio`, `ActionDate`, `ActionBy`, `Action`
    ) VALUES (
      OLD.`ItemsId`, OLD.`ItemsSubId`, OLD.`ItemUuid`, OLD.`BuyingPrice`, OLD.`MarkedPrice`, OLD.`RetailPrice`, OLD.`WholesalePrice`, OLD.`RetailDiscountRatio`, OLD.`WholesaleDiscountRatio`, NOW(), NEW.`UpdatedBy`, 'UPDATE'
    );
  END IF;
END$$
DELIMITER ;
