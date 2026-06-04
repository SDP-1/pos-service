-- need to apply in prod

-- SQL Code for Inventory Adjustment Audit Table and Triggers
-- This file contains the database schema and triggers for tracking inventory adjustments

-- ============================================================================
-- 1. CREATE InventoryAdjustAudits TABLE
-- ============================================================================
CREATE TABLE `inventoryadjustaudits` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `InventoryUuid` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ItemUuid` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `PreviousQuantity` decimal(18,3) NOT NULL,
  `NewQuantity` decimal(18,3) NOT NULL,
  `AdjustmentQuantity` decimal(18,3) NOT NULL,
  `UnitType` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Increase` tinyint(1) NOT NULL,
  `Comment` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Reason` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `UpdatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  `UpdatedBy` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_InventoryAdjustAudits_InventoryUuid` (`InventoryUuid`),
  KEY `IX_InventoryAdjustAudits_ItemUuid` (`ItemUuid`),
  KEY `IX_InventoryAdjustAudits_UpdatedBy` (`UpdatedBy`),
  CONSTRAINT `FK_InventoryAdjustAudits_Inventories_InventoryUuid` FOREIGN KEY (`InventoryUuid`) REFERENCES `inventories` (`Uuid`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_InventoryAdjustAudits_Users_UpdatedBy` FOREIGN KEY (`UpdatedBy`) REFERENCES `users` (`Uuid`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci


-- ============================================================================
-- 2. UPDATE inventories TABLE - Add Comment, Reason, and IsUserAdjusted columns
-- ============================================================================
ALTER TABLE `inventories` 
ADD COLUMN `Comment` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
ADD COLUMN `Reason` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
ADD COLUMN `IsUserAdjusted` tinyint(1) NOT NULL DEFAULT 0;

-- ============================================================================
-- 3. CREATE TRIGGER - Log inventory adjustments when UPDATE occurs (only if IsUserAdjusted = true)
-- ============================================================================
DROP TRIGGER IF EXISTS `pos-system`.`trg_inventories_audit_on_update`;

DELIMITER $$
USE `pos-system`$$
CREATE DEFINER=`root`@`localhost` TRIGGER `trg_inventories_audit_on_update` AFTER UPDATE ON `inventories` FOR EACH ROW BEGIN
	DECLARE adjustment_qty DECIMAL(18,3);
	DECLARE is_increase TINYINT(1);

	-- Only record audit if IsUserAdjusted is true (manual user adjustment)
	IF NEW.IsUserAdjusted = 1 AND NEW.StockQuantity != OLD.StockQuantity THEN
		-- Calculate adjustment quantity and direction
		SET adjustment_qty = NEW.StockQuantity - OLD.StockQuantity;
		SET is_increase = IF(adjustment_qty > 0, 1, 0);

		-- Insert audit record
		INSERT INTO `inventoryAdjustAudits` (
			`InventoryUuid`,
			`ItemUuid`,
			`PreviousQuantity`,
			`NewQuantity`,
			`AdjustmentQuantity`,
			`UnitType`,
			`Increase`,
			`Comment`,
			`Reason`,
			`UpdatedAt`,
			`UpdatedBy`
		) VALUES (
			NEW.Uuid,
			NEW.ItemUuid,
			OLD.StockQuantity,
			NEW.StockQuantity,
			adjustment_qty,
			NEW.UnitType,
			is_increase,
			NEW.Comment,
			NEW.Reason,
			CURRENT_TIMESTAMP(6),
			NEW.UpdatedBy
		); 
	END IF;
END$$
DELIMITER ;

-- ============================================================================
-- 5. HELPER PROCEDURE - Get inventory adjustment history by item UUID
-- ============================================================================
-- Parameters:
-- p_item_uuid: ItemUuid (REQUIRED)
-- p_start_date: Start date filter (OPTIONAL - NULL to ignore)
-- p_end_date: End date filter (OPTIONAL - NULL to ignore)
-- p_max_records: Maximum number of records to return (OPTIONAL - default 100)
-- ============================================================================
USE `pos-system`;
DROP procedure IF EXISTS `sp_get_inventory_audit_history`;

USE `pos-system`;
DROP procedure IF EXISTS `pos-system`.`sp_get_inventory_audit_history`;
;

DELIMITER $$
USE `pos-system`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_get_inventory_audit_history`(
    IN p_item_uuid VARCHAR(36),
    IN p_start_date DATETIME,
    IN p_end_date DATETIME,
    IN p_max_records INT
)
BEGIN
    -- fallback if NULL passed
    IF p_max_records IS NULL THEN
        SET p_max_records = 10;
    END IF;

    SELECT 
        a.InventoryUuid,
        a.ItemUuid,
        a.PreviousQuantity,
        a.NewQuantity,
        a.AdjustmentQuantity,
        a.UnitType,
        IF(a.Increase = 1, 'Increase', 'Decrease') AS AdjustmentType,
        a.Comment,
        a.Reason,
        a.UpdatedAt,
        CONCAT(u.FirstName, ' ', u.LastName) AS UpdatedByUser,
        a.UpdatedBy
    FROM inventoryadjustaudits a
    LEFT JOIN users u ON a.UpdatedBy = u.Uuid
    WHERE a.ItemUuid = p_item_uuid
      AND (p_start_date IS NULL OR a.UpdatedAt >= p_start_date)
      AND (p_end_date IS NULL OR a.UpdatedAt <= p_end_date)
    ORDER BY a.UpdatedAt DESC
    LIMIT p_max_records;

END$$

DELIMITER ;
;




-- ============================================================================
-- NOTES:
-- ============================================================================
-- 1. The trigger automatically logs inventory adjustments to the audit table ONLY when IsUserAdjusted = true
-- 2. IsUserAdjusted is true for: manual adjustments via AdjustStockAsync and item edit operations
-- 3. IsUserAdjusted is false for: initial inventory creation and automatic operations like order processing
-- 4. The audit table tracks: previous qty, new qty, adjustment amount, direction (increase/decrease)
-- 5. Comment and Reason are captured from the inventories table
-- 6. All audit records are timestamped and linked to the user who made the change
-- 7. sp_get_inventory_audit_history procedure provides flexible querying:
--    - p_item_uuid: REQUIRED - filter by item
--    - p_start_date: OPTIONAL - filter by start date (NULL to ignore)
--    - p_end_date: OPTIONAL - filter by end date (NULL to ignore)
--    - p_max_records: OPTIONAL - limit results (default 100)
-- 8. All date/record filters are optional for maximum flexibility
