-- ====================================================================================
-- TASK SQL SCRIPT: Task 6
-- DESCRIPTION: Batch-Driven Pricing & Inventory Architecture Transition
-- ====================================================================================
SET SQL_SAFE_UPDATES = 0;
-- 1. CREATE CORE BATCH & PURCHASING TABLES
-- ====================================================================================

CREATE TABLE IF NOT EXISTS `tbl_purchases` (
  `Id`              int NOT NULL AUTO_INCREMENT,
  `Uuid`            varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `PurchaseNumber`  varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `SupplierUuid`    varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `InvoiceNumber`   varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `PurchaseDate`    date NOT NULL,
  `TotalCost`       decimal(18,2) NOT NULL DEFAULT '0.00',
  `TotalItems`      int NOT NULL DEFAULT '0',
  `Status`          varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT 'Received',
  `Notes`           varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `CreatedAt`       datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt`       datetime(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  `CreatedBy`       varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `UpdatedBy`       varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `IsActive`        tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `AK_tbl_purchases_Uuid` (`Uuid`),
  UNIQUE KEY `IX_tbl_purchases_PurchaseNumber` (`PurchaseNumber`),
  KEY `IX_tbl_purchases_SupplierUuid` (`SupplierUuid`),
  KEY `IX_tbl_purchases_PurchaseDate` (`PurchaseDate`),
  KEY `FK_tbl_purchases_tbl_users_CreatedBy` (`CreatedBy`),
  KEY `FK_tbl_purchases_tbl_users_UpdatedBy` (`UpdatedBy`),
  CONSTRAINT `FK_tbl_purchases_tbl_suppliers_SupplierUuid` FOREIGN KEY (`SupplierUuid`) REFERENCES `tbl_suppliers` (`Uuid`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `FK_tbl_purchases_tbl_users_CreatedBy` FOREIGN KEY (`CreatedBy`) REFERENCES `tbl_users` (`Uuid`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `FK_tbl_purchases_tbl_users_UpdatedBy` FOREIGN KEY (`UpdatedBy`) REFERENCES `tbl_users` (`Uuid`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `tbl_inventory_batches` (
  `Id`                     int NOT NULL AUTO_INCREMENT,
  `Uuid`                   varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ItemUuid`               varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `BatchNumber`            varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,

  -- Quantities
  `ReceivedQuantity`       decimal(18,3) NOT NULL,
  `RemainingQuantity`      decimal(18,3) NOT NULL,

  -- Batch-level pricing
  `CostPrice`              decimal(18,2) NOT NULL DEFAULT '0.00',
  `MarkedPrice`            decimal(18,2) NOT NULL DEFAULT '0.00',
  `RetailPrice`            decimal(18,2) NOT NULL DEFAULT '0.00',
  `WholesalePrice`         decimal(18,2) NOT NULL DEFAULT '0.00',
  `RetailDiscountRatio`    decimal(5,2) NOT NULL DEFAULT '0.00',
  `WholesaleDiscountRatio` decimal(5,2) NOT NULL DEFAULT '0.00',

  -- Reference (GRN / Invoice, Max 200)
  `Reference`              varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `PurchaseUuid`           varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `SupplierUuid`           varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,

  -- Status & Audit
  `Status`                 varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT 'Active',
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

CREATE TABLE IF NOT EXISTS `tbl_stock_movements` (
  `Id`              bigint NOT NULL AUTO_INCREMENT,
  `Uuid`            varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `BatchUuid`       varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ItemUuid`        varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `MovementType`    varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Quantity`        decimal(18,3) NOT NULL,
  `Direction`       varchar(3) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CostPrice`       decimal(18,2) NOT NULL DEFAULT '0.00',
  `ReferenceType`   varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ReferenceUuid`   varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'UUID of referenced Order, Purchase, Adjustment or Return',
  `Reason`          varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Comment`         varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `CreatedAt`       datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `CreatedBy`       varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `AK_tbl_stock_movements_Uuid` (`Uuid`),
  KEY `IX_tbl_stock_movements_BatchUuid` (`BatchUuid`),
  KEY `IX_tbl_stock_movements_ItemUuid` (`ItemUuid`),
  KEY `IX_tbl_stock_movements_MovementType` (`MovementType`),
  KEY `IX_tbl_stock_movements_CreatedAt` (`CreatedAt`),
  KEY `FK_tbl_stock_movements_tbl_users_CreatedBy` (`CreatedBy`),
  CONSTRAINT `FK_tbl_stock_movements_tbl_inventory_batches_BatchUuid` FOREIGN KEY (`BatchUuid`) REFERENCES `tbl_inventory_batches` (`Uuid`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_tbl_stock_movements_tbl_items_ItemUuid` FOREIGN KEY (`ItemUuid`) REFERENCES `tbl_items` (`Uuid`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_tbl_stock_movements_tbl_users_CreatedBy` FOREIGN KEY (`CreatedBy`) REFERENCES `tbl_users` (`Uuid`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

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


-- 2. ALTER TABLES & SCHEMA CONSOLIDATION
-- ====================================================================================

-- Add Description directly to tbl_items if not exists
SET @dbname = DATABASE();
SET @tablename = 'tbl_items';

SET @columnname = 'Description';
SET @preparedStatement = (SELECT IF(
  (
    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tablename AND COLUMN_NAME = @columnname
  ) > 0,
  'SELECT 1',
  'ALTER TABLE `tbl_items` ADD COLUMN `Description` VARCHAR(500) NULL AFTER `BarCode`;'
));
PREPARE alterItemDesc FROM @preparedStatement;
EXECUTE alterItemDesc;
DEALLOCATE PREPARE alterItemDesc;

-- Drop UnitType from tbl_items (units and base unit are maintained exclusively in tbl_item_units)
SET @columnname = 'UnitType';
SET @preparedStatement = (SELECT IF(
  (
    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tablename AND COLUMN_NAME = @columnname
  ) > 0,
  'ALTER TABLE `tbl_items` DROP COLUMN `UnitType`;',
  'SELECT 1'
));
PREPARE dropItemUnitType FROM @preparedStatement;
EXECUTE dropItemUnitType;
DEALLOCATE PREPARE dropItemUnitType;

SET @columnname = 'AllowsDecimalQuantities';
SET @preparedStatement = (SELECT IF(
  (
    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tablename AND COLUMN_NAME = @columnname
  ) > 0,
  'SELECT 1',
  'ALTER TABLE `tbl_items` ADD COLUMN `AllowsDecimalQuantities` TINYINT(1) NOT NULL DEFAULT \'0\';'
));
PREPARE alterItemDecimals FROM @preparedStatement;
EXECUTE alterItemDecimals;
DEALLOCATE PREPARE alterItemDecimals;

-- Migrate AllowsDecimalQuantities from tbl_inventories into tbl_items if tbl_inventories exists
SET @preparedStatement = (SELECT IF(
  (
    SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = 'tbl_inventories'
  ) > 0,
  'UPDATE `tbl_items` i JOIN `tbl_inventories` inv ON i.`Uuid` = inv.`ItemUuid` SET i.`AllowsDecimalQuantities` = COALESCE(inv.`AllowsDecimalQuantities`, 0);',
  'SELECT 1'
));
PREPARE copyInvAttributes FROM @preparedStatement;
EXECUTE copyInvAttributes;
DEALLOCATE PREPARE copyInvAttributes;

-- Create tbl_item_units (Item packaging & unit hierarchy directly linked to tbl_items)
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

-- Migrate records from tbl_inventory_units into tbl_item_units if tbl_inventory_units exists
SET @hasIsBaseUnit = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = 'tbl_inventory_units' AND COLUMN_NAME = 'IsBaseUnit'
);

SET @selectBaseUnitExpr = IF(@hasIsBaseUnit > 0, 'u.`IsBaseUnit`', '0');

SET @preparedStatement = (SELECT IF(
  (
    SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = 'tbl_inventory_units'
  ) > 0 AND (
    SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = 'tbl_inventories'
  ) > 0,
  CONCAT(
    'INSERT INTO `tbl_item_units` (`ItemUuid`, `UnitType`, `ParentUnitType`, `QuantityPerParent`, `QuantityInBaseUnits`, `IsBaseUnit`, `Uuid`) ',
    'SELECT inv.`ItemUuid`, u.`UnitType`, u.`ParentUnitType`, u.`QuantityPerParent`, u.`QuantityInBaseUnits`, ', @selectBaseUnitExpr, ', u.`Uuid` ',
    'FROM `tbl_inventory_units` u JOIN `tbl_inventories` inv ON u.`InventoryId` = inv.`Id` ',
    'WHERE NOT EXISTS (SELECT 1 FROM `tbl_item_units` iu WHERE iu.`Uuid` = u.`Uuid`);'
  ),
  'SELECT 1'
));
PREPARE copyUnitsToItemUnits FROM @preparedStatement;
EXECUTE copyUnitsToItemUnits;
DEALLOCATE PREPARE copyUnitsToItemUnits;

-- Ensure every item has a base unit record in tbl_item_units
SET @preparedStatement = (SELECT IF(
  (
    SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = 'tbl_inventories'
  ) > 0,
  'INSERT INTO `tbl_item_units` (`ItemUuid`, `UnitType`, `ParentUnitType`, `QuantityPerParent`, `QuantityInBaseUnits`, `IsBaseUnit`, `Uuid`)
   SELECT i.`Uuid`, COALESCE(inv.`UnitType`, \'Each\'), COALESCE(inv.`UnitType`, \'Each\'), 1.000, 1.000, 1, UUID()
   FROM `tbl_items` i
   LEFT JOIN `tbl_inventories` inv ON i.`Uuid` = inv.`ItemUuid`
   WHERE NOT EXISTS (
     SELECT 1 FROM `tbl_item_units` iu WHERE iu.`ItemUuid` = i.`Uuid` AND iu.`IsBaseUnit` = 1
   );',
  'INSERT INTO `tbl_item_units` (`ItemUuid`, `UnitType`, `ParentUnitType`, `QuantityPerParent`, `QuantityInBaseUnits`, `IsBaseUnit`, `Uuid`)
   SELECT i.`Uuid`, \'Each\', \'Each\', 1.000, 1.000, 1, UUID()
   FROM `tbl_items` i
   WHERE NOT EXISTS (
     SELECT 1 FROM `tbl_item_units` iu WHERE iu.`ItemUuid` = i.`Uuid` AND iu.`IsBaseUnit` = 1
   );'
));
PREPARE seedBaseItemUnits FROM @preparedStatement;
EXECUTE seedBaseItemUnits;
DEALLOCATE PREPARE seedBaseItemUnits;

-- Add BatchUuid to tbl_order_items if not exists
SET @tablename = 'tbl_order_items';
SET @columnname = 'BatchUuid';
SET @preparedStatement = (SELECT IF(
  (
    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE
      TABLE_SCHEMA = @dbname
      AND TABLE_NAME = @tablename
      AND COLUMN_NAME = @columnname
  ) > 0,
  'SELECT 1',
  'ALTER TABLE `tbl_order_items` ADD COLUMN `BatchUuid` VARCHAR(36) NULL AFTER `OriginalItemUuid`, ADD KEY `IX_tbl_order_items_BatchUuid` (`BatchUuid`), ADD CONSTRAINT `FK_tbl_order_items_tbl_inventory_batches_BatchUuid` FOREIGN KEY (`BatchUuid`) REFERENCES `tbl_inventory_batches` (`Uuid`) ON DELETE SET NULL ON UPDATE CASCADE;'
));
PREPARE alterIfNotExists FROM @preparedStatement;
EXECUTE alterIfNotExists;
DEALLOCATE PREPARE alterIfNotExists;

SET @fkname = 'FK_tbl_order_items_tbl_order_items_ReturnedOrderItemUuid';
SET @preparedStatementFK = (SELECT IF(
  (
    SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE
      TABLE_SCHEMA = @dbname
      AND TABLE_NAME = @tablename
      AND CONSTRAINT_NAME = @fkname
  ) > 0,
  'SELECT 1',
  'ALTER TABLE `tbl_order_items` ADD KEY `IX_tbl_order_items_ReturnedOrderItemUuid` (`ReturnedOrderItemUuid`), ADD CONSTRAINT `FK_tbl_order_items_tbl_order_items_ReturnedOrderItemUuid` FOREIGN KEY (`ReturnedOrderItemUuid`) REFERENCES `tbl_order_items` (`Uuid`) ON DELETE SET NULL ON UPDATE CASCADE;'
));
PREPARE alterFKIfNotExists FROM @preparedStatementFK;
EXECUTE alterFKIfNotExists;
DEALLOCATE PREPARE alterFKIfNotExists;


-- 3. SEED DEFAULT BATCH FOR EVERY EXISTING ITEM
-- ====================================================================================

-- Provision default active batch for each item from legacy tables if not already present
INSERT INTO `tbl_inventory_batches` (
    `Uuid`, `ItemUuid`, `BatchNumber`,
    `ReceivedQuantity`, `RemainingQuantity`,
    `CostPrice`, `MarkedPrice`, `RetailPrice`, `WholesalePrice`, `RetailDiscountRatio`, `WholesaleDiscountRatio`,
    `Reference`,
    `SupplierUuid`, `Status`,
    `CreatedAt`, `CreatedBy`, `IsActive`
)
SELECT
    UUID() AS `Uuid`,
    inv.`ItemUuid`,
    CONCAT('BATCH-INIT-', LPAD(i.`Id`, 5, '0')) AS `BatchNumber`,
    inv.`StockQuantity` AS `ReceivedQuantity`,
    inv.`StockQuantity` AS `RemainingQuantity`,
    COALESCE(ip.`BuyingPrice`, 0.00) AS `CostPrice`,
    COALESCE(ip.`MarkedPrice`, 0.00) AS `MarkedPrice`,
    COALESCE(ip.`RetailPrice`, 0.00) AS `RetailPrice`,
    COALESCE(ip.`WholesalePrice`, 0.00) AS `WholesalePrice`,
    COALESCE(ip.`RetailDiscountRatio`, 0.00) AS `RetailDiscountRatio`,
    COALESCE(ip.`WholesaleDiscountRatio`, 0.00) AS `WholesaleDiscountRatio`,
    'Initial Opening Lot' AS `Reference`,
    (SELECT s.`Uuid` FROM `tbl_item_suppliers` isu JOIN `tbl_suppliers` s ON isu.`SuppliersId` = s.`Id` WHERE isu.`ItemsId` = i.`Id` AND isu.`ItemsSubId` = i.`SubId` LIMIT 1) AS `SupplierUuid`,
    'Active' AS `Status`,
    NOW(6), inv.`CreatedBy`, 1
FROM `tbl_inventories` inv
JOIN `tbl_items` i ON inv.`ItemUuid` = i.`Uuid`
LEFT JOIN `tbl_item_prices` ip ON ip.`ItemUuid` = inv.`ItemUuid`
WHERE inv.`IsActive` = 1
  AND NOT EXISTS (
    SELECT 1 FROM `tbl_inventory_batches` b WHERE b.`ItemUuid` = inv.`ItemUuid`
  );

-- Create OpeningStock movement entries for newly seeded initial batches
INSERT INTO `tbl_stock_movements` (
    `Uuid`, `BatchUuid`, `ItemUuid`, `MovementType`,
    `Quantity`, `Direction`, `CostPrice`,
    `Reason`, `Comment`, `CreatedAt`, `CreatedBy`
)
SELECT
    UUID(), b.`Uuid`, b.`ItemUuid`, 'OpeningStock',
    b.`ReceivedQuantity`, 'IN', b.`CostPrice`,
    'Migrated opening stock from initial inventory', 'Initial system migration', NOW(6), b.`CreatedBy`
FROM `tbl_inventory_batches` b
JOIN `tbl_inventories` inv ON b.`ItemUuid` = inv.`ItemUuid`
WHERE b.`BatchNumber` LIKE 'BATCH-INIT-%'
  AND NOT EXISTS (
    SELECT 1 FROM `tbl_stock_movements` sm WHERE sm.`BatchUuid` = b.`Uuid` AND sm.`MovementType` = 'OpeningStock'
  );


-- 4. TRIGGERS ON tbl_inventory_batches
-- ====================================================================================

-- Drop any previous before/after triggers
DROP TRIGGER IF EXISTS `trg_tbl_inventory_batches_before_insert`;
DROP TRIGGER IF EXISTS `trg_tbl_inventory_batches_before_update`;

-- Trigger: After Insert on tbl_inventory_batches (A.INSERT)
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

-- Trigger: After Update on tbl_inventory_batches (A.UPDATE)
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


-- 5. CLEANUP OBSOLETE TRIGGERS & TABLES
-- ====================================================================================

-- Drop obsolete triggers on legacy price tables
DROP TRIGGER IF EXISTS `trg_tbl_item_prices_after_insert`;
DROP TRIGGER IF EXISTS `trg_tbl_item_prices_after_update`;

-- Drop obsolete audit tables (superseded by tbl_inventory_batch_logs & tbl_stock_movements)
DROP TABLE IF EXISTS `tbl_item_price_audits`;
DROP TABLE IF EXISTS `tbl_inventory_adjust_audits`;

-- Drop obsolete item price table (All pricing is exclusively managed on tbl_inventory_batches)
DROP TABLE IF EXISTS `tbl_item_prices`;

-- Drop obsolete inventory units table (Migrated and replaced by tbl_item_units)
DROP TABLE IF EXISTS `tbl_inventory_units`;

-- Drop obsolete inventory table (UnitType and AllowsDecimalQuantities now live on tbl_items, and true stock is on tbl_inventory_batches)
DROP TABLE IF EXISTS `tbl_inventories`;

SET SQL_SAFE_UPDATES = 1;

-- Applied on to prod 2026-08-22