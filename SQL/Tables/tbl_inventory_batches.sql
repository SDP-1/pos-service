-- ====================================================================================
-- TABLE SCHEMA: tbl_inventory_batches
-- INCLUDES TRIGGERS:
--   - trg_tbl_inventory_batches_after_insert (A.INSERT)
--   - trg_tbl_inventory_batches_after_update (A.UPDATE)
-- ====================================================================================

CREATE TABLE IF NOT EXISTS `tbl_inventory_batches` (
  `Id`                     int NOT NULL AUTO_INCREMENT,
  `Uuid`                   varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ItemUuid`               varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `BatchNumber`            varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,

  -- Quantities
  `ReceivedQuantity`       decimal(18,3) NOT NULL,
  `RemainingQuantity`      decimal(18,3) NOT NULL,

  -- Full batch-level pricing
  `CostPrice`              decimal(18,2) NOT NULL DEFAULT '0.00',
  `MarkedPrice`            decimal(18,2) NOT NULL DEFAULT '0.00',
  `RetailPrice`            decimal(18,2) NOT NULL DEFAULT '0.00',
  `WholesalePrice`         decimal(18,2) NOT NULL DEFAULT '0.00',
  `RetailDiscountRatio`    decimal(5,2) NOT NULL DEFAULT '0.00',
  `WholesaleDiscountRatio` decimal(5,2) NOT NULL DEFAULT '0.00',

  -- Reference (Max 200)
  `Reference`              varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  
  -- References
  `PurchaseUuid`           varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `SupplierUuid`           varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,

  -- Status
  `Status`                 varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT 'Active',

  -- IAuditable
  `CreatedAt`              datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt`              datetime(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  `CreatedBy`              varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `UpdatedBy`              varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `IsActive`               tinyint(1) NOT NULL DEFAULT '1',

  PRIMARY KEY (`Id`),
  UNIQUE KEY `AK_tbl_inventory_batches_Uuid` (`Uuid`),
  KEY `IX_tbl_inventory_batches_ItemUuid` (`ItemUuid`),
  KEY `IX_tbl_inventory_batches_PurchaseUuid` (`PurchaseUuid`),
  KEY `IX_tbl_inventory_batches_SupplierUuid` (`SupplierUuid`),
  KEY `IX_tbl_inventory_batches_Status` (`Status`),
  KEY `IX_tbl_inventory_batches_ItemUuid_Status` (`ItemUuid`, `Status`),
  KEY `FK_tbl_inventory_batches_tbl_users_CreatedBy` (`CreatedBy`),
  KEY `FK_tbl_inventory_batches_tbl_users_UpdatedBy` (`UpdatedBy`),
  CONSTRAINT `FK_tbl_inventory_batches_tbl_items_ItemUuid` FOREIGN KEY (`ItemUuid`) REFERENCES `tbl_items` (`Uuid`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_tbl_inventory_batches_tbl_purchases_PurchaseUuid` FOREIGN KEY (`PurchaseUuid`) REFERENCES `tbl_purchases` (`Uuid`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `FK_tbl_inventory_batches_tbl_suppliers_SupplierUuid` FOREIGN KEY (`SupplierUuid`) REFERENCES `tbl_suppliers` (`Uuid`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `FK_tbl_inventory_batches_tbl_users_CreatedBy` FOREIGN KEY (`CreatedBy`) REFERENCES `tbl_users` (`Uuid`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `FK_tbl_inventory_batches_tbl_users_UpdatedBy` FOREIGN KEY (`UpdatedBy`) REFERENCES `tbl_users` (`Uuid`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;


-- ====================================================================================
-- TRIGGERS ON tbl_inventory_batches
-- ====================================================================================

-- Drop any previous before/after triggers
DROP TRIGGER IF EXISTS `trg_tbl_inventory_batches_before_insert`;
DROP TRIGGER IF EXISTS `trg_tbl_inventory_batches_before_update`;

-- 1. Trigger: After Insert (A.INSERT)
DROP TRIGGER IF EXISTS `trg_tbl_inventory_batches_after_insert`;
DELIMITER $$
CREATE TRIGGER `trg_tbl_inventory_batches_after_insert`
AFTER INSERT ON `tbl_inventory_batches`
FOR EACH ROW
BEGIN
  INSERT INTO `tbl_inventory_batch_logs`
  SELECT 
    NULL,
    b.*,
    'A.INSERT',
    NOW(6),
    NEW.`CreatedBy`
  FROM `tbl_inventory_batches` b
  WHERE b.`Id` = NEW.`Id`;
END$$
DELIMITER ;

-- 2. Trigger: After Update (A.UPDATE)
DROP TRIGGER IF EXISTS `trg_tbl_inventory_batches_after_update`;
DELIMITER $$
CREATE TRIGGER `trg_tbl_inventory_batches_after_update`
AFTER UPDATE ON `tbl_inventory_batches`
FOR EACH ROW
BEGIN
  INSERT INTO `tbl_inventory_batch_logs`
  SELECT 
    NULL,
    b.*,
    'A.UPDATE',
    NOW(6),
    NEW.`UpdatedBy`
  FROM `tbl_inventory_batches` b
  WHERE b.`Id` = NEW.`Id`;
END$$
DELIMITER ;
