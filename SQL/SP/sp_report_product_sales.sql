DROP PROCEDURE IF EXISTS `sp_report_product_sales`;
DELIMITER //
CREATE PROCEDURE `sp_report_product_sales`(IN p_StartDate DATETIME, IN p_EndDate DATETIME)
BEGIN
    SELECT 
        oi.PrintName AS ItemName,
        COALESCE(i.BarCode, '') AS BarCode,
        SUM(oi.Quantity) AS TotalQuantitySold,
        COALESCE(u.UnitType, 'Each') AS UnitType,
        SUM(oi.LineTotal) AS TotalRevenue,
        SUM(oi.CostAtSale * oi.Quantity) AS TotalCost,
        SUM(oi.LineTotal - (oi.CostAtSale * oi.Quantity)) AS TotalProfit
    FROM tbl_order_items oi
    JOIN tbl_orders o ON oi.OrderId = o.Id
    LEFT JOIN tbl_items i ON oi.OriginalItemUuid = i.Uuid
    LEFT JOIN tbl_item_units u ON i.Uuid = u.ItemUuid AND u.IsBaseUnit = 1
    WHERE o.IsActive = 1
      AND (p_StartDate IS NULL OR DATE(o.CreatedAt) >= DATE(p_StartDate))
      AND (p_EndDate IS NULL OR DATE(o.CreatedAt) <= DATE(p_EndDate))
    GROUP BY oi.PrintName, i.BarCode, u.UnitType
    ORDER BY TotalRevenue DESC;
END //
DELIMITER ;

-- Applied on prod 2026-08-23