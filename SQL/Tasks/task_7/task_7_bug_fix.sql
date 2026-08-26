SET SQL_SAFE_UPDATES = 0;
-- ====================================================================================
-- TASK SQL SCRIPT: Task 7 Bug Fix
-- DESCRIPTION: Fix tbl_order_logs & tbl_order_item_logs table schema and triggers to
--              match tbl_orders & tbl_order_items column list and order 1-to-1.
-- ====================================================================================

-- 1. DROP EXISTING TRIGGERS & RECREATE tbl_order_logs TABLE
-- ====================================================================================
DROP TRIGGER IF EXISTS `trg_tbl_orders_after_insert`;
DROP TRIGGER IF EXISTS `trg_tbl_orders_after_update`;
DROP TRIGGER IF EXISTS `trg_tbl_orders_after_delete`;
DROP TRIGGER IF EXISTS `trg_tbl_orders_before_delete`;

DROP TABLE IF EXISTS `tbl_order_logs`;

CREATE TABLE `tbl_order_logs` (
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

-- ====================================================================================
-- 2. DROP EXISTING TRIGGERS & RECREATE tbl_order_item_logs TABLE
-- ====================================================================================
DROP TRIGGER IF EXISTS `trg_tbl_order_items_after_insert`;
DROP TRIGGER IF EXISTS `trg_tbl_order_items_after_update`;
DROP TRIGGER IF EXISTS `trg_tbl_order_items_after_delete`;
DROP TRIGGER IF EXISTS `trg_tbl_order_items_before_delete`;

DROP TABLE IF EXISTS `tbl_order_item_logs`;

CREATE TABLE `tbl_order_item_logs` (
  `LogId`                  bigint NOT NULL AUTO_INCREMENT,
  `OrderItemId`            int NOT NULL,
  `OrderId`                int NOT NULL,
  `OriginalItemUuid`       varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `BatchUuid`              varchar(36) DEFAULT NULL,
  `AllowsDecimalQuantities` tinyint(1) NOT NULL,
  `PrintName`              varchar(40) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Quantity`               decimal(18,3) NOT NULL,
  `PriceAtSale`            decimal(18,2) NOT NULL,
  `MarkedPriceAtSale`      decimal(18,2) NOT NULL DEFAULT '0.00',
  `CostAtSale`             decimal(18,2) NOT NULL,
  `LineTotal`              decimal(18,2) NOT NULL,
  `IsReturnItem`           tinyint(1) NOT NULL DEFAULT '0',
  `ReturnedOrderItemUuid`  varchar(36) DEFAULT NULL,
  `Description`            varchar(500) DEFAULT NULL,
  `OrderItemUuid`          varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CreatedAt`              datetime(6) NOT NULL,
  `UpdatedAt`              datetime(6) DEFAULT NULL,
  `CreatedBy`              varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `UpdatedBy`              varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `IsActive`               tinyint(1) NOT NULL DEFAULT '1',
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

-- ====================================================================================
-- 3. TRIGGERS ON tbl_orders
-- ====================================================================================
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

-- ====================================================================================
-- 4. TRIGGERS ON tbl_order_items
-- ====================================================================================
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

SET SQL_SAFE_UPDATES = 1;

-- Applied on Prod 2026-08-24