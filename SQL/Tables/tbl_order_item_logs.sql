-- ====================================================================================
-- TABLE SCHEMA: tbl_order_item_logs
-- AUDIT LOG FOR ORDER LINE ITEMS (INSERT, UPDATE, DELETE)
-- STRUCTURE ALIGNED WITH tbl_order_items (oi.*) + (Action, ActionDate, ActionBy)
-- ====================================================================================

CREATE TABLE IF NOT EXISTS `tbl_order_item_logs` (
  `LogId`                  bigint NOT NULL AUTO_INCREMENT,
  `OrderItemId`            int NOT NULL,
  `OrderId`                int NOT NULL,
  `OriginalItemUuid`       varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `BatchUuid`              varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `AllowsDecimalQuantities` tinyint(1) NOT NULL,
  `PrintName`              varchar(40) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Quantity`               decimal(18,3) NOT NULL,
  `PriceAtSale`            decimal(18,2) NOT NULL,
  `MarkedPriceAtSale`      decimal(18,2) NOT NULL DEFAULT '0.00',
  `CostAtSale`             decimal(18,2) NOT NULL,
  `LineTotal`              decimal(18,2) NOT NULL,
  `IsReturnItem`           tinyint(1) NOT NULL DEFAULT '0',
  `ReturnedOrderItemUuid`  varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Description`            varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
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
-- TRIGGERS ON tbl_order_items
-- ====================================================================================

-- 1. Trigger: After Insert (A.INSERT)
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

-- 2. Trigger: After Update (A.UPDATE)
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

-- 3. Trigger: Before Delete (A.DELETE)
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
