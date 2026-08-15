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
        COALESCE(pa.RetailPrice, ip.RetailPrice, 0.00) AS SellingPrice,
        COALESCE(pa.BuyingPrice, ip.BuyingPrice, 0.00) AS CostPrice,
        COALESCE(inv.StockQuantity * COALESCE(pa.RetailPrice, ip.RetailPrice, 0.00), 0.00) AS TotalSellingValue,
        COALESCE(inv.StockQuantity * COALESCE(pa.BuyingPrice, ip.BuyingPrice, 0.00), 0.00) AS TotalCostValue
    FROM tbl_items i
    JOIN tbl_inventories inv ON i.Uuid = inv.ItemUuid
    LEFT JOIN tbl_item_prices ip ON i.Uuid = ip.ItemUuid
    LEFT JOIN (
        SELECT a1.*
        FROM tbl_item_price_audits a1
        INNER JOIN (
            SELECT ItemUuid, MAX(ChangedAt) AS MaxChangedAt, MAX(Id) AS MaxId
            FROM tbl_item_price_audits
            GROUP BY ItemUuid
        ) a2 ON a1.ItemUuid = a2.ItemUuid AND a1.ChangedAt = a2.MaxChangedAt AND a1.Id = a2.MaxId
    ) pa ON i.Uuid = pa.ItemUuid
    WHERE i.IsActive = 1 AND inv.IsActive = 1
    ORDER BY i.PrintName ASC;
END //
DELIMITER ;
