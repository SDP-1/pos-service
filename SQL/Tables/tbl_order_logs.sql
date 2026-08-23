-- ====================================================================================
-- TABLE SCHEMA: tbl_order_logs
-- AUDIT LOG FOR ORDERS (INSERT, UPDATE, DELETE)
-- STRUCTURE ALIGNED WITH tbl_orders (o.*) + (Action, ActionDate, ActionBy)
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

-- ====================================================================================
-- TRIGGERS ON tbl_orders
-- ====================================================================================

-- 1. Trigger: After Insert (A.INSERT)
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

-- 2. Trigger: After Update (A.UPDATE)
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

-- 3. Trigger: Before Delete (A.DELETE)
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
