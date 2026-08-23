DROP PROCEDURE IF EXISTS `sp_report_current_stock`;
DELIMITER //
CREATE PROCEDURE `sp_report_current_stock`()
BEGIN
    SELECT 
        i.Id AS ItemId,
        i.PrintName AS ItemName,
        COALESCE(i.BarCode, '') AS BarCode,
        COALESCE(b_agg.StockQuantity, 0.000) AS StockQuantity,
        COALESCE(u.UnitType, 'Each') AS UnitType,
        COALESCE(b_agg.AvgRetailPrice, 0.00) AS SellingPrice,
        COALESCE(b_agg.AvgCostPrice, 0.00) AS CostPrice,
        COALESCE(b_agg.TotalSellingValue, 0.00) AS TotalSellingValue,
        COALESCE(b_agg.TotalCostValue, 0.00) AS TotalCostValue
    FROM tbl_items i
    LEFT JOIN (
        SELECT 
            b.ItemUuid,
            SUM(b.RemainingQuantity) AS StockQuantity,
            SUM(b.RemainingQuantity * b.RetailPrice) AS TotalSellingValue,
            SUM(b.RemainingQuantity * b.CostPrice) AS TotalCostValue,
            CASE WHEN SUM(b.RemainingQuantity) > 0 THEN SUM(b.RemainingQuantity * b.RetailPrice) / SUM(b.RemainingQuantity) ELSE 0.00 END AS AvgRetailPrice,
            CASE WHEN SUM(b.RemainingQuantity) > 0 THEN SUM(b.RemainingQuantity * b.CostPrice) / SUM(b.RemainingQuantity) ELSE 0.00 END AS AvgCostPrice
        FROM tbl_inventory_batches b
        WHERE b.IsActive = 1 AND b.RemainingQuantity > 0
        GROUP BY b.ItemUuid
    ) b_agg ON i.Uuid = b_agg.ItemUuid
    LEFT JOIN tbl_item_units u ON i.Uuid = u.ItemUuid AND u.IsBaseUnit = 1
    WHERE i.IsActive = 1
    ORDER BY i.PrintName ASC;
END //
DELIMITER ;

-- Applied on prod 2026-08-23