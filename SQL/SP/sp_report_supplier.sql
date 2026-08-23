DROP PROCEDURE IF EXISTS `sp_report_supplier`;
DELIMITER //
CREATE PROCEDURE `sp_report_supplier`()
BEGIN
    SELECT 
        s.Id AS SupplierId,
        s.Name AS SupplierName,
        COUNT(DISTINCT isu.ItemsId) AS TotalSuppliedItems,
        COALESCE(SUM(inv.StockQuantity), 0.000) AS TotalStockQuantity,
        COALESCE(SUM(COALESCE(pa.BuyingPrice, ip.BuyingPrice, 0.00) * inv.StockQuantity), 0.00) AS TotalInventoryValue
    FROM tbl_suppliers s
    LEFT JOIN tbl_item_suppliers isu ON s.Id = isu.SuppliersId
    LEFT JOIN tbl_items i ON isu.ItemsId = i.Id AND isu.ItemsSubId = i.SubId AND i.IsActive = 1
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
    LEFT JOIN tbl_inventories inv ON i.Uuid = inv.ItemUuid AND inv.IsActive = 1
    WHERE s.IsActive = 1
    GROUP BY s.Id, s.Name
    ORDER BY s.Name ASC;
END //
DELIMITER ;

-- Created on 2026-07-31
-- Applied on dev 2026-07-31
-- Applied on prod 2026-08-23