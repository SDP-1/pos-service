-- ============================================================================
-- Task Migration Script: Rename Database Tables to Snake Case with 'tbl_' Prefix
-- Date: 2026-07-31
-- Description: Alters existing table names to snake_case format starting with 'tbl_'.
--              Updates database Views, Triggers, and Stored Procedures accordingly.
-- ============================================================================

USE `pos-system`;

-- Disable foreign key checks during renaming
SET FOREIGN_KEY_CHECKS = 0;

-- 1. Rename Existing Tables to tbl_<snake_case> Format
RENAME TABLE `users` TO `tbl_users`;
RENAME TABLE `contacts` TO `tbl_contacts`;
RENAME TABLE `customers` TO `tbl_customers`;
RENAME TABLE `suppliers` TO `tbl_suppliers`;
RENAME TABLE `items` TO `tbl_items`;
RENAME TABLE `itemprices` TO `tbl_item_prices`;
RENAME TABLE `itemexpiries` TO `tbl_item_expiries`;
RENAME TABLE `itemsuppliers` TO `tbl_item_suppliers`;
RENAME TABLE `orders` TO `tbl_orders`;
RENAME TABLE `orderitems` TO `tbl_order_items`;
RENAME TABLE `permissions` TO `tbl_permissions`;
RENAME TABLE `rolepermissions` TO `tbl_role_permissions`;
RENAME TABLE `roles` TO `tbl_roles`;
RENAME TABLE `settings` TO `tbl_settings`;
RENAME TABLE `backuplocations` TO `tbl_backup_locations`;
RENAME TABLE `backuphistories` TO `tbl_backup_histories`;
RENAME TABLE `shops` TO `tbl_shops`;
RENAME TABLE `loansettlementlogs` TO `tbl_loan_settlement_logs`;
RENAME TABLE `inventories` TO `tbl_inventories`;
RENAME TABLE `inventoryunits` TO `tbl_inventory_units`;
RENAME TABLE `inventoryadjustaudits` TO `tbl_inventory_adjust_audits`;
RENAME TABLE `reporttemplates` TO `tbl_report_templates`;
RENAME TABLE `sqltemplates` TO `tbl_sql_templates`;
RENAME TABLE `reporttemplatesqltemplates` TO `tbl_report_template_sql_templates`;

-- Re-enable foreign key checks
SET FOREIGN_KEY_CHECKS = 1;


-- ============================================================================
-- 2. Update Database Views
-- View: view_returned_items_summary
-- ============================================================================
DROP VIEW IF EXISTS `view_returned_items_summary`;

CREATE OR REPLACE VIEW `view_returned_items_summary` AS
    SELECT 
        `o`.`Id` AS `OrderId`,
        `o`.`OrderNumber` AS `OrderNumber`,
        `o`.`Uuid` AS `OrderUuid`,
        `original_item`.`PrintName` AS `PrintName`,
        `original_item`.`Uuid` AS `ReturnedOrderItemUuid`,
        `original_item`.`Quantity` AS `OriginalPurchasedQty`,
        SUM(`return_item`.`Quantity`) AS `TotalReturnedQty`,
        (`original_item`.`Quantity` - SUM(`return_item`.`Quantity`)) AS `RemainingQty`,
        `original_item`.`PriceAtSale` AS `PriceAtSale`,
        CAST((SUM(`return_item`.`Quantity`) * `original_item`.`PriceAtSale`)
            AS DECIMAL (18 , 2 )) AS `TotalRefundAmountValue`
    FROM
        ((`tbl_order_items` `return_item`
        JOIN `tbl_order_items` `original_item` ON ((`return_item`.`ReturnedOrderItemUuid` = `original_item`.`Uuid`)))
        JOIN `tbl_orders` `o` ON ((`original_item`.`OrderId` = `o`.`Id`)))
    WHERE
        (`return_item`.`IsReturnItem` = 1)
    GROUP BY `o`.`Id` , `o`.`OrderNumber` , `o`.`Uuid` , `original_item`.`Uuid` , `original_item`.`PrintName` , `original_item`.`Quantity` , `original_item`.`PriceAtSale`;


-- ============================================================================
-- 3. Update Triggers
-- Trigger: trg_inventories_audit_on_update
-- ============================================================================
DROP TRIGGER IF EXISTS `trg_inventories_audit_on_update`;

DELIMITER $$
CREATE TRIGGER `trg_inventories_audit_on_update` AFTER UPDATE ON `tbl_inventories` FOR EACH ROW BEGIN
	DECLARE adjustment_qty DECIMAL(18,3);
	DECLARE is_increase TINYINT(1);

	-- Only record audit if IsUserAdjusted is true (manual user adjustment)
	IF NEW.IsUserAdjusted = 1 AND NEW.StockQuantity != OLD.StockQuantity THEN
		-- Calculate adjustment quantity and direction
		SET adjustment_qty = NEW.StockQuantity - OLD.StockQuantity;
		SET is_increase = IF(adjustment_qty > 0, 1, 0);

		-- Insert audit record into tbl_inventory_adjust_audits
		INSERT INTO `tbl_inventory_adjust_audits` (
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
-- 4. Update Stored Procedures
-- Procedure: sp_get_inventory_audit_history
-- ============================================================================
DROP PROCEDURE IF EXISTS `sp_get_inventory_audit_history`;

DELIMITER $$
CREATE PROCEDURE `sp_get_inventory_audit_history`(
    IN p_item_uuid VARCHAR(36),
    IN p_start_date DATETIME,
    IN p_end_date DATETIME,
    IN p_max_records INT
)
BEGIN
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
    FROM tbl_inventory_adjust_audits a
    LEFT JOIN tbl_users u ON a.UpdatedBy = u.Uuid
    WHERE a.ItemUuid = p_item_uuid
      AND (p_start_date IS NULL OR a.UpdatedAt >= p_start_date)
      AND (p_end_date IS NULL OR a.UpdatedAt <= p_end_date)
    ORDER BY a.UpdatedAt DESC
    LIMIT p_max_records;

END$$
DELIMITER ;

-- Apply on dev on 2026-07-31
-- Need to apply on prod