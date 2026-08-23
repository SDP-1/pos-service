DROP PROCEDURE IF EXISTS `sp_report_low_stock`;
DELIMITER //
CREATE PROCEDURE `sp_report_low_stock`()
BEGIN
    SELECT 
        i.Id AS ItemId,
        i.PrintName AS ItemName,
        COALESCE(i.BarCode, '') AS BarCode,
        COALESCE(inv.StockQuantity, 0.000) AS StockQuantity,
        COALESCE(inv.UnitType, 'Piece') AS UnitType,
        5.000 AS LowStockThreshold,
        COALESCE(pa.RetailPrice, ip.RetailPrice, 0.00) AS SellingPrice
    FROM tbl_items i
    JOIN tbl_inventories inv ON i.Uuid = inv.ItemUuid
    LEFT JOIN tbl_item_prices ip ON i.Uuid = ip.ItemUuid
    LEFT JOIN (
        SELECT a1.*
        FROM tbl_item_price_audits a1
        INNER JOIN (
            SELECT ItemUuid, MAX(ActionDate) AS MaxActionDate, MAX(Id) AS MaxId
            FROM tbl_item_price_audits
            GROUP BY ItemUuid
        ) a2 ON a1.ItemUuid = a2.ItemUuid AND a1.ActionDate = a2.MaxActionDate AND a1.Id = a2.MaxId
    ) pa ON i.Uuid = pa.ItemUuid
    WHERE i.IsActive = 1 AND inv.IsActive = 1 AND inv.StockQuantity <= 5.000
    ORDER BY inv.StockQuantity ASC;
END //
DELIMITER ;
