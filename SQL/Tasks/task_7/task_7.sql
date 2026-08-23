SET SQL_SAFE_UPDATES = 0;
-- ====================================================================================
-- TASK SQL SCRIPT: Task 7
-- DESCRIPTION: Order Deletion Reversals, Order Logs & Order Item Logs
-- ====================================================================================

-- 1. CREATE tbl_order_logs TABLE
-- ====================================================================================
CREATE TABLE IF NOT EXISTS `tbl_order_logs` (
  `LogId`         bigint NOT NULL AUTO_INCREMENT,
  `OrderId`       int NOT NULL,
  `OrderNumber`   varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `MainStatus`    varchar(50) NOT NULL,
  `SubStatus`     varchar(50) DEFAULT NULL,
  `PaymentMethod` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `SaleType`      varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ItemCount`     int NOT NULL,
  `GrossAmount`   decimal(18,2) NOT NULL,
  `TotalDiscount` decimal(18,2) NOT NULL,
  `NetAmount`     decimal(18,2) NOT NULL,
  `TotalCost`     decimal(18,2) NOT NULL,
  `AmountPaid`    decimal(18,2) NOT NULL,
  `Balance`       decimal(18,2) NOT NULL,
  `CashierId`     int DEFAULT NULL,
  `CustomerId`    int DEFAULT NULL,
  `Description`   longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `OrderUuid`     varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CreatedAt`     datetime(6) NOT NULL,
  `UpdatedAt`     datetime(6) DEFAULT NULL,
  `CreatedBy`     varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `UpdatedBy`     varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `IsActive`      tinyint(1) NOT NULL DEFAULT '1',
  `Action`        varchar(10) NOT NULL,
  `ActionDate`    datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `ActionBy`      varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  PRIMARY KEY (`LogId`),
  KEY `IX_tbl_order_logs_OrderId` (`OrderId`),
  KEY `IX_tbl_order_logs_OrderUuid` (`OrderUuid`),
  KEY `IX_tbl_order_logs_Action` (`Action`),
  KEY `IX_tbl_order_logs_ActionDate` (`ActionDate`),
  KEY `FK_tbl_order_logs_tbl_users_ActionBy` (`ActionBy`),
  CONSTRAINT `FK_tbl_order_logs_tbl_users_ActionBy` FOREIGN KEY (`ActionBy`) REFERENCES `tbl_users` (`Uuid`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 2. CREATE tbl_order_item_logs TABLE
-- ====================================================================================
CREATE TABLE IF NOT EXISTS `tbl_order_item_logs` (
  `LogId`                  bigint NOT NULL AUTO_INCREMENT,
  `OrderItemId`            int NOT NULL,
  `OrderId`                int NOT NULL,
  `OriginalItemUuid`       varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `BatchUuid`              varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `PrintName`              varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `BarCode`                varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `PriceAtSale`            decimal(18,2) NOT NULL,
  `CostAtSale`             decimal(18,2) NOT NULL,
  `Quantity`               decimal(18,3) NOT NULL,
  `Discount`               decimal(18,2) NOT NULL DEFAULT '0.00',
  `LineTotal`              decimal(18,2) NOT NULL,
  `TotalProfit`            decimal(18,2) NOT NULL DEFAULT '0.00',
  `UnitType`               varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT 'Each',
  `IsReturnItem`           tinyint(1) NOT NULL DEFAULT '0',
  `ReturnedOrderItemUuid`  varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `OrderItemUuid`          varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Action`                 varchar(10) NOT NULL,
  `ActionDate`             datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `ActionBy`               varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  PRIMARY KEY (`LogId`),
  KEY `IX_tbl_order_item_logs_OrderItemId` (`OrderItemId`),
  KEY `IX_tbl_order_item_logs_OrderId` (`OrderId`),
  KEY `IX_tbl_order_item_logs_OriginalItemUuid` (`OriginalItemUuid`),
  KEY `IX_tbl_order_item_logs_OrderItemUuid` (`OrderItemUuid`),
  KEY `IX_tbl_order_item_logs_Action` (`Action`),
  KEY `IX_tbl_order_item_logs_ActionDate` (`ActionDate`),
  KEY `FK_tbl_order_item_logs_tbl_users_ActionBy` (`ActionBy`),
  CONSTRAINT `FK_tbl_order_item_logs_tbl_users_ActionBy` FOREIGN KEY (`ActionBy`) REFERENCES `tbl_users` (`Uuid`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 3. TRIGGERS ON tbl_orders
-- ====================================================================================
DROP TRIGGER IF EXISTS `trg_tbl_orders_after_insert`;
DELIMITER $$
CREATE TRIGGER `trg_tbl_orders_after_insert`
AFTER INSERT ON `tbl_orders`
FOR EACH ROW
BEGIN
  INSERT INTO `tbl_order_logs`
  SELECT 
    NULL,
    o.*,
    'A.INSERT',
    NOW(6),
    NEW.`CreatedBy`
  FROM `tbl_orders` o
  WHERE o.`Id` = NEW.`Id`;
END$$
DELIMITER ;

DROP TRIGGER IF EXISTS `trg_tbl_orders_after_update`;
DELIMITER $$
CREATE TRIGGER `trg_tbl_orders_after_update`
AFTER UPDATE ON `tbl_orders`
FOR EACH ROW
BEGIN
  INSERT INTO `tbl_order_logs`
  SELECT 
    NULL,
    o.*,
    'A.UPDATE',
    NOW(6),
    COALESCE(NEW.`UpdatedBy`, NEW.`CreatedBy`)
  FROM `tbl_orders` o
  WHERE o.`Id` = NEW.`Id`;
END$$
DELIMITER ;

DROP TRIGGER IF EXISTS `trg_tbl_orders_after_delete`;
DROP TRIGGER IF EXISTS `trg_tbl_orders_before_delete`;
DELIMITER $$
CREATE TRIGGER `trg_tbl_orders_before_delete`
BEFORE DELETE ON `tbl_orders`
FOR EACH ROW
BEGIN
  INSERT INTO `tbl_order_logs`
  SELECT 
    NULL,
    o.*,
    'A.DELETE',
    NOW(6),
    COALESCE(OLD.`UpdatedBy`, OLD.`CreatedBy`)
  FROM `tbl_orders` o
  WHERE o.`Id` = OLD.`Id`;
END$$
DELIMITER ;

-- 4. TRIGGERS ON tbl_order_items
-- ====================================================================================
DROP TRIGGER IF EXISTS `trg_tbl_order_items_after_insert`;
DELIMITER $$
CREATE TRIGGER `trg_tbl_order_items_after_insert`
AFTER INSERT ON `tbl_order_items`
FOR EACH ROW
BEGIN
  DECLARE v_ActionBy VARCHAR(36);
  SELECT COALESCE(o.`UpdatedBy`, o.`CreatedBy`) INTO v_ActionBy FROM `tbl_orders` o WHERE o.`Id` = NEW.`OrderId` LIMIT 1;

  INSERT INTO `tbl_order_item_logs`
  SELECT 
    NULL,
    oi.*,
    'A.INSERT',
    NOW(6),
    v_ActionBy
  FROM `tbl_order_items` oi
  WHERE oi.`Id` = NEW.`Id`;
END$$
DELIMITER ;

DROP TRIGGER IF EXISTS `trg_tbl_order_items_after_update`;
DELIMITER $$
CREATE TRIGGER `trg_tbl_order_items_after_update`
AFTER UPDATE ON `tbl_order_items`
FOR EACH ROW
BEGIN
  DECLARE v_ActionBy VARCHAR(36);
  SELECT COALESCE(o.`UpdatedBy`, o.`CreatedBy`) INTO v_ActionBy FROM `tbl_orders` o WHERE o.`Id` = NEW.`OrderId` LIMIT 1;

  INSERT INTO `tbl_order_item_logs`
  SELECT 
    NULL,
    oi.*,
    'A.UPDATE',
    NOW(6),
    v_ActionBy
  FROM `tbl_order_items` oi
  WHERE oi.`Id` = NEW.`Id`;
END$$
DELIMITER ;

DROP TRIGGER IF EXISTS `trg_tbl_order_items_after_delete`;
DROP TRIGGER IF EXISTS `trg_tbl_order_items_before_delete`;
DELIMITER $$
CREATE TRIGGER `trg_tbl_order_items_before_delete`
BEFORE DELETE ON `tbl_order_items`
FOR EACH ROW
BEGIN
  DECLARE v_ActionBy VARCHAR(36);
  SELECT COALESCE(o.`UpdatedBy`, o.`CreatedBy`) INTO v_ActionBy FROM `tbl_orders` o WHERE o.`Id` = OLD.`OrderId` LIMIT 1;

  INSERT INTO `tbl_order_item_logs`
  SELECT 
    NULL,
    oi.*,
    'A.DELETE',
    NOW(6),
    v_ActionBy
  FROM `tbl_order_items` oi
  WHERE oi.`Id` = OLD.`Id`;
END$$
DELIMITER ;

-- ====================================================================================
-- 5. DATA MIGRATION: UPDATE tbl_stock_movements (ReferenceType & MovementType TO CAPITAL_SNAKE_CASE)
-- ====================================================================================
UPDATE `tbl_stock_movements`
SET `ReferenceType` = CASE `ReferenceType`
    WHEN 'Purchase'        THEN 'PURCHASE'
    WHEN 'Order'           THEN 'ORDER'
    WHEN 'OrderReturn'     THEN 'ORDER_RETURN'
    WHEN 'OrderDelete'     THEN 'ORDER_DELETE'
    WHEN 'ManualBatch'     THEN 'MANUAL_BATCH'
    WHEN 'StockAdjustment' THEN 'STOCK_ADJUSTMENT'
    WHEN 'Transfer'        THEN 'TRANSFER'
    WHEN 'OpeningStock'    THEN 'OPENING_STOCK'
    ELSE UPPER(`ReferenceType`)
END
WHERE `ReferenceType` IS NOT NULL;

UPDATE `tbl_stock_movements`
SET `MovementType` = CASE `MovementType`
    WHEN 'Purchase'        THEN 'PURCHASE'
    WHEN 'Sale'            THEN 'SALE'
    WHEN 'SaleReturn'      THEN 'SALE_RETURN'
    WHEN 'PurchaseReturn'  THEN 'PURCHASE_RETURN'
    WHEN 'DamageWriteOff'  THEN 'DAMAGE_WRITE_OFF'
    WHEN 'ExpiryWriteOff'  THEN 'EXPIRY_WRITE_OFF'
    WHEN 'ManualAdjustIn'  THEN 'MANUAL_ADJUST_IN'
    WHEN 'ManualAdjustOut' THEN 'MANUAL_ADJUST_OUT'
    WHEN 'OpeningStock'    THEN 'OPENING_STOCK'
    WHEN 'Transfer'        THEN 'TRANSFER'
    WHEN 'StockCount'      THEN 'STOCK_COUNT'
    ELSE UPPER(`MovementType`)
END
WHERE `MovementType` IS NOT NULL;

SET SQL_SAFE_UPDATES = 1;

-- Applied on prod 2026-08-23