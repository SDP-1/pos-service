DROP PROCEDURE IF EXISTS `sp_report_supplier`;
DELIMITER //
CREATE PROCEDURE `sp_report_supplier`()
BEGIN
    SELECT 
        s.Id AS SupplierId,
        s.Name AS SupplierName,
        COUNT(DISTINCT isu.ItemsId) AS TotalSuppliedItems,
        COALESCE(SUM(b.RemainingQuantity), 0.000) AS TotalStockQuantity,
        COALESCE(SUM(b.RemainingQuantity * b.CostPrice), 0.00) AS TotalInventoryValue
    FROM tbl_suppliers s
    LEFT JOIN tbl_item_suppliers isu ON s.Id = isu.SuppliersId
    LEFT JOIN tbl_items i ON isu.ItemsId = i.Id AND isu.ItemsSubId = i.SubId AND i.IsActive = 1
    LEFT JOIN tbl_inventory_batches b ON i.Uuid = b.ItemUuid AND b.IsActive = 1 AND b.RemainingQuantity > 0
    WHERE s.IsActive = 1
    GROUP BY s.Id, s.Name
    ORDER BY s.Name ASC;
END //
DELIMITER ;

-- Applied on prod 2026-08-23