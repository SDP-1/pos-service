DROP PROCEDURE IF EXISTS `sp_report_low_stock`;
DELIMITER //
CREATE PROCEDURE `sp_report_low_stock`()
BEGIN
    SELECT 
        i.Id AS ItemId,
        i.PrintName AS ItemName,
        COALESCE(i.BarCode, '') AS BarCode,
        COALESCE(b_agg.StockQuantity, 0.000) AS StockQuantity,
        COALESCE(u.UnitType, 'Each') AS UnitType,
        5.000 AS LowStockThreshold,
        COALESCE(b_agg.AvgRetailPrice, 0.00) AS SellingPrice
    FROM tbl_items i
    LEFT JOIN (
        SELECT 
            b.ItemUuid,
            SUM(b.RemainingQuantity) AS StockQuantity,
            CASE WHEN SUM(b.RemainingQuantity) > 0 THEN SUM(b.RemainingQuantity * b.RetailPrice) / SUM(b.RemainingQuantity) ELSE 0.00 END AS AvgRetailPrice
        FROM tbl_inventory_batches b
        WHERE b.IsActive = 1 AND b.RemainingQuantity > 0
        GROUP BY b.ItemUuid
    ) b_agg ON i.Uuid = b_agg.ItemUuid
    LEFT JOIN tbl_item_units u ON i.Uuid = u.ItemUuid AND u.IsBaseUnit = 1
    WHERE i.IsActive = 1 AND COALESCE(b_agg.StockQuantity, 0.000) <= 5.000
    ORDER BY COALESCE(b_agg.StockQuantity, 0.000) ASC;
END //
DELIMITER ;

-- Applied on prod 2026-08-23