DROP PROCEDURE IF EXISTS `sp_report_category_sales`;
DELIMITER //
CREATE PROCEDURE `sp_report_category_sales`(IN p_StartDate DATETIME, IN p_EndDate DATETIME)
BEGIN
    SELECT 
        COALESCE(u.UnitType, 'General') AS CategoryName,
        COUNT(DISTINCT oi.OriginalItemUuid) AS UniqueItemsCount,
        SUM(oi.Quantity) AS TotalQuantitySold,
        SUM(oi.LineTotal) AS TotalRevenue,
        SUM(oi.LineTotal - (oi.CostAtSale * oi.Quantity)) AS TotalProfit
    FROM tbl_order_items oi
    JOIN tbl_orders o ON oi.OrderId = o.Id
    LEFT JOIN tbl_items i ON oi.OriginalItemUuid = i.Uuid
    LEFT JOIN tbl_item_units u ON i.Uuid = u.ItemUuid AND u.IsBaseUnit = 1
    WHERE o.IsActive = 1
      AND (p_StartDate IS NULL OR DATE(o.CreatedAt) >= DATE(p_StartDate))
      AND (p_EndDate IS NULL OR DATE(o.CreatedAt) <= DATE(p_EndDate))
    GROUP BY u.UnitType
    ORDER BY TotalRevenue DESC;
END //
DELIMITER ;

-- Applied on prod 2026-08-23