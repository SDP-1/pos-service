DROP PROCEDURE IF EXISTS `sp_report_current_stock`;
DELIMITER //
CREATE PROCEDURE `sp_report_current_stock`()
BEGIN
    SELECT 
        i.Id AS ItemId,
        i.PrintName AS ItemName,
        COALESCE(i.BarCode, '') AS BarCode,
        COALESCE(inv.StockQuantity, 0.000) AS StockQuantity,
        COALESCE(inv.UnitType, 'Piece') AS UnitType,
        COALESCE(batch_val.AvgRetailPrice, pa.RetailPrice, ip.RetailPrice, 0.00) AS SellingPrice,
        COALESCE(batch_val.AvgCostPrice, pa.BuyingPrice, ip.BuyingPrice, 0.00) AS CostPrice,
        COALESCE(batch_val.TotalSellingValue, inv.StockQuantity * COALESCE(pa.RetailPrice, ip.RetailPrice, 0.00), 0.00) AS TotalSellingValue,
        COALESCE(batch_val.TotalCostValue, inv.StockQuantity * COALESCE(pa.BuyingPrice, ip.BuyingPrice, 0.00), 0.00) AS TotalCostValue
    FROM tbl_items i
    JOIN tbl_inventories inv ON i.Uuid = inv.ItemUuid
    LEFT JOIN tbl_item_prices ip ON i.Uuid = ip.ItemUuid
    LEFT JOIN (
        SELECT 
            b.ItemUuid,
            SUM(b.RemainingQuantity * b.RetailPrice) AS TotalSellingValue,
            SUM(b.RemainingQuantity * b.CostPrice) AS TotalCostValue,
            CASE WHEN SUM(b.RemainingQuantity) > 0 THEN SUM(b.RemainingQuantity * b.RetailPrice) / SUM(b.RemainingQuantity) ELSE 0.00 END AS AvgRetailPrice,
            CASE WHEN SUM(b.RemainingQuantity) > 0 THEN SUM(b.RemainingQuantity * b.CostPrice) / SUM(b.RemainingQuantity) ELSE 0.00 END AS AvgCostPrice
        FROM tbl_inventory_batches b
        WHERE b.IsActive = 1 AND b.RemainingQuantity > 0 AND (b.ExpiryDate IS NULL OR b.ExpiryDate >= CURDATE())
        GROUP BY b.ItemUuid
    ) batch_val ON i.Uuid = batch_val.ItemUuid
    LEFT JOIN (
        SELECT a1.*
        FROM tbl_item_price_audits a1
        INNER JOIN (
            SELECT ItemUuid, MAX(ActionDate) AS MaxActionDate, MAX(Id) AS MaxId
            FROM tbl_item_price_audits
            GROUP BY ItemUuid
        ) a2 ON a1.ItemUuid = a2.ItemUuid AND a1.ActionDate = a2.MaxActionDate AND a1.Id = a2.MaxId
    ) pa ON i.Uuid = pa.ItemUuid
    WHERE i.IsActive = 1 AND inv.IsActive = 1
    ORDER BY i.PrintName ASC;
END //
DELIMITER ;
