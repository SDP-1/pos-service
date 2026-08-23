DROP PROCEDURE IF EXISTS `sp_report_purchase`;
DELIMITER //
CREATE PROCEDURE `sp_report_purchase`(IN p_StartDate DATETIME, IN p_EndDate DATETIME)
BEGIN
    SELECT 
        DATE(sm.CreatedAt)                                                                            AS PurchaseDate,
        COALESCE(i.PrintName, 'Unknown Item')                                                         AS ItemName,
        COALESCE(i.BarCode, '')                                                                       AS BarCode,
        COALESCE(s.Name, 'No Supplier')                                                               AS SupplierName,
        sm.Quantity                                                                                   AS QuantityPurchased,
        COALESCE(u_unit.UnitType, 'Each')                                                             AS UnitType,
        COALESCE(sm.CostPrice, b.CostPrice, 0.00)                                                     AS UnitCost,
        COALESCE(ROUND(sm.Quantity * COALESCE(sm.CostPrice, b.CostPrice, 0.00), 2), 0.00)             AS TotalValue,
        COALESCE(NULLIF(TRIM(CONCAT(u.FirstName, ' ', IFNULL(u.LastName, ''))), ''), u.UserName, 'System') AS RecordedBy,
        COALESCE(sm.Reason, '-')                                                                      AS Reason,
        COALESCE(sm.Comment, '-')                                                                     AS Comment
    FROM tbl_stock_movements sm
    JOIN tbl_items i ON sm.ItemUuid = i.Uuid AND i.IsActive = 1
    LEFT JOIN tbl_inventory_batches b ON sm.BatchUuid = b.Uuid
    LEFT JOIN tbl_purchases p ON b.PurchaseUuid = p.Uuid
    LEFT JOIN tbl_suppliers s ON COALESCE(b.SupplierUuid, p.SupplierUuid) = s.Uuid
    LEFT JOIN tbl_item_units u_unit ON i.Uuid = u_unit.ItemUuid AND u_unit.IsBaseUnit = 1
    LEFT JOIN tbl_users u ON sm.CreatedBy = u.Uuid
    WHERE sm.MovementType IN ('PURCHASE')
      AND (p_StartDate IS NULL OR DATE(sm.CreatedAt) >= DATE(p_StartDate))
      AND (p_EndDate IS NULL OR DATE(sm.CreatedAt) <= DATE(p_EndDate))
    ORDER BY sm.CreatedAt DESC;
END //
DELIMITER ;

-- Applied on prod 2026-08-23