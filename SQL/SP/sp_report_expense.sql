DROP PROCEDURE IF EXISTS `sp_report_expense`;
DELIMITER //
CREATE PROCEDURE `sp_report_expense`(IN p_StartDate DATETIME, IN p_EndDate DATETIME)
BEGIN
    SELECT 
        DATE(audit.UpdatedAt) AS ExpenseDate,
        COALESCE(audit.Reason, 'Stock Adjustment Expense') AS ExpenseReason,
        COALESCE(audit.Comment, '-') AS Comment,
        i.PrintName AS ItemName,
        audit.AdjustmentQuantity,
        audit.UnitType,
        COALESCE(NULLIF(TRIM(CONCAT(u.FirstName, ' ', IFNULL(u.LastName, ''))), ''), u.UserName, 'System') AS User
    FROM tbl_inventory_adjust_audits audit
    JOIN tbl_inventories inv ON audit.InventoryUuid = inv.Uuid
    JOIN tbl_items i ON inv.ItemUuid = i.Uuid
    LEFT JOIN tbl_users u ON audit.UpdatedBy = u.Uuid
    WHERE audit.AdjustmentQuantity < 0
      AND (p_StartDate IS NULL OR DATE(audit.UpdatedAt) >= DATE(p_StartDate))
      AND (p_EndDate IS NULL OR DATE(audit.UpdatedAt) <= DATE(p_EndDate))
    ORDER BY audit.UpdatedAt DESC;
END //
DELIMITER ;
