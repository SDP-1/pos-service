DROP PROCEDURE IF EXISTS `sp_report_expense`;
DELIMITER //
CREATE PROCEDURE `sp_report_expense`(IN p_StartDate DATETIME, IN p_EndDate DATETIME)
BEGIN
    SELECT 
        DATE(sm.CreatedAt) AS ExpenseDate,
        COALESCE(sm.Reason, sm.MovementType, 'Stock Adjustment Expense') AS ExpenseReason,
        COALESCE(sm.Comment, '-') AS Comment,
        COALESCE(i.PrintName, 'Unknown Item') AS ItemName,
        sm.Quantity AS AdjustmentQuantity,
        COALESCE(u_unit.UnitType, 'Each') AS UnitType,
        COALESCE(NULLIF(TRIM(CONCAT(u.FirstName, ' ', IFNULL(u.LastName, ''))), ''), u.UserName, 'System') AS User
    FROM tbl_stock_movements sm
    JOIN tbl_items i ON sm.ItemUuid = i.Uuid
    LEFT JOIN tbl_item_units u_unit ON i.Uuid = u_unit.ItemUuid AND u_unit.IsBaseUnit = 1
    LEFT JOIN tbl_users u ON sm.CreatedBy = u.Uuid
    WHERE sm.Direction = 'OUT' AND sm.MovementType IN ('DAMAGE_WRITE_OFF', 'EXPIRY_WRITE_OFF', 'MANUAL_ADJUST_OUT', 'PURCHASE_RETURN')
      AND (p_StartDate IS NULL OR DATE(sm.CreatedAt) >= DATE(p_StartDate))
      AND (p_EndDate IS NULL OR DATE(sm.CreatedAt) <= DATE(p_EndDate))
    ORDER BY sm.CreatedAt DESC;
END //
DELIMITER ;

-- Applied on prod 2026-08-23