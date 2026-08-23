-- Stored Procedure: sp_get_inventory_audit_history
-- Description: Fetches inventory adjustment history records for a given item UUID.

USE `pos-system`;

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

-- Created on 2026-07-31
-- Applied on dev 2026-07-31
-- Applied on prod 2026-08-23