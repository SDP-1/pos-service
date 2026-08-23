DROP PROCEDURE IF EXISTS `sp_report_sales_return`;
DELIMITER //
CREATE PROCEDURE `sp_report_sales_return`(IN p_StartDate DATETIME, IN p_EndDate DATETIME)
BEGIN
    SELECT 
        o.OrderNumber,
        DATE(o.CreatedAt) AS OrderDate,
        oi.PrintName AS ItemName,
        oi.Quantity AS ReturnedQuantity,
        COALESCE(u.UnitType, 'Each') AS UnitType,
        oi.LineTotal AS RefundAmount,
        COALESCE(o.Description, 'Customer Return') AS ReturnReason
    FROM tbl_order_items oi
    JOIN tbl_orders o ON oi.OrderId = o.Id
    LEFT JOIN tbl_items i ON oi.OriginalItemUuid = i.Uuid
    LEFT JOIN tbl_item_units u ON i.Uuid = u.ItemUuid AND u.IsBaseUnit = 1
    WHERE o.IsActive = 1 AND (oi.IsReturnItem = 1 OR o.SubStatus = 'Return')
      AND (p_StartDate IS NULL OR DATE(o.CreatedAt) >= DATE(p_StartDate))
      AND (p_EndDate IS NULL OR DATE(o.CreatedAt) <= DATE(p_EndDate))
    ORDER BY o.CreatedAt DESC;
END //
DELIMITER ;

-- Applied on prod 2026-08-23