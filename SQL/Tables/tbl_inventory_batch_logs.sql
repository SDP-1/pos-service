-- ====================================================================================
-- TABLE SCHEMA: tbl_inventory_batch_logs
-- AUDIT LOG FOR INVENTORY BATCH CREATION AND MODIFICATIONS
-- STRUCTURE ALIGNED WITH tbl_inventory_batches (b.*) + (Action, ActionDate, ActionBy)
-- ====================================================================================

-- Ensure tbl_inventory_batch_logs schema is up to date (drop if missing LogId from older schema)
SET @dbname = DATABASE();
SET @tablename = 'tbl_inventory_batch_logs';
SET @columnname = 'LogId';
SET @preparedStatement = (SELECT IF(
  (
    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tablename AND COLUMN_NAME = @columnname
  ) = 0 AND (
    SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tablename
  ) > 0,
  'DROP TABLE `tbl_inventory_batch_logs`;',
  'SELECT 1'
));
PREPARE dropOldBatchLogs FROM @preparedStatement;
EXECUTE dropOldBatchLogs;
DEALLOCATE PREPARE dropOldBatchLogs;

CREATE TABLE IF NOT EXISTS `tbl_inventory_batch_logs` (
  `LogId`                  bigint NOT NULL AUTO_INCREMENT,
  `BatchId`                int NOT NULL,
  `BatchUuid`              varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ItemUuid`               varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `BatchNumber`            varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ReceivedQuantity`       decimal(18,3) NOT NULL,
  `RemainingQuantity`      decimal(18,3) NOT NULL,
  `CostPrice`              decimal(18,2) NOT NULL DEFAULT '0.00',
  `MarkedPrice`            decimal(18,2) NOT NULL DEFAULT '0.00',
  `RetailPrice`            decimal(18,2) NOT NULL DEFAULT '0.00',
  `WholesalePrice`         decimal(18,2) NOT NULL DEFAULT '0.00',
  `RetailDiscountRatio`    decimal(5,2) NOT NULL DEFAULT '0.00',
  `WholesaleDiscountRatio` decimal(5,2) NOT NULL DEFAULT '0.00',
  `Reference`              varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `PurchaseUuid`           varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `SupplierUuid`           varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Status`                 varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CreatedAt`              datetime(6) NOT NULL,
  `UpdatedAt`              datetime(6) DEFAULT NULL,
  `CreatedBy`              varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `UpdatedBy`              varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `IsActive`               tinyint(1) NOT NULL DEFAULT '1',
  `Action`                 varchar(10) NOT NULL,
  `ActionDate`             datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `ActionBy`               varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  PRIMARY KEY (`LogId`),
  KEY `IX_tbl_inventory_batch_logs_BatchUuid` (`BatchUuid`),
  KEY `IX_tbl_inventory_batch_logs_ItemUuid` (`ItemUuid`),
  KEY `IX_tbl_inventory_batch_logs_Action` (`Action`),
  KEY `IX_tbl_inventory_batch_logs_ActionDate` (`ActionDate`),
  CONSTRAINT `FK_tbl_inventory_batch_logs_tbl_items_ItemUuid` FOREIGN KEY (`ItemUuid`) REFERENCES `tbl_items` (`Uuid`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_tbl_inventory_batch_logs_tbl_inventory_batches_BatchUuid` FOREIGN KEY (`BatchUuid`) REFERENCES `tbl_inventory_batches` (`Uuid`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_tbl_inventory_batch_logs_tbl_users_ActionBy` FOREIGN KEY (`ActionBy`) REFERENCES `tbl_users` (`Uuid`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
