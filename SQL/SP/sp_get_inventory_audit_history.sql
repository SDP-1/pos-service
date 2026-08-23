-- Stored Procedure: sp_get_inventory_audit_history
-- Description: Fetches inventory adjustment history records for a given item UUID from tbl_stock_movements.

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
        COALESCE(sm.BatchUuid, sm.Uuid) AS InventoryUuid,
        sm.ItemUuid,
        0.000 AS PreviousQuantity,
        sm.Quantity AS NewQuantity,
        sm.Quantity AS AdjustmentQuantity,
        COALESCE(u_unit.UnitType, 'Each') AS UnitType,
        IF(sm.Direction = 'IN', 'Increase', 'Decrease') AS AdjustmentType,
        sm.Comment,
        COALESCE(sm.Reason, sm.MovementType) AS Reason,
        sm.CreatedAt AS UpdatedAt,
        CONCAT(u.FirstName, ' ', IFNULL(u.LastName, '')) AS UpdatedByUser,
        sm.CreatedBy AS UpdatedBy
    FROM tbl_stock_movements sm
    LEFT JOIN tbl_items i ON sm.ItemUuid = i.Uuid
    LEFT JOIN tbl_item_units u_unit ON i.Uuid = u_unit.ItemUuid AND u_unit.IsBaseUnit = 1
    LEFT JOIN tbl_users u ON sm.CreatedBy = u.Uuid
    WHERE sm.ItemUuid = p_item_uuid
      AND (p_start_date IS NULL OR sm.CreatedAt >= p_start_date)
      AND (p_end_date IS NULL OR sm.CreatedAt <= p_end_date)
    ORDER BY sm.CreatedAt DESC
    LIMIT p_max_records;

END$$
DELIMITER ;

-- Applied on prod 2026-08-23