DROP PROCEDURE IF EXISTS `sp_report_purchase`;
DELIMITER //
CREATE PROCEDURE `sp_report_purchase`(IN p_StartDate DATETIME, IN p_EndDate DATETIME)
BEGIN
    SELECT 
        DATE(audit.UpdatedAt)                                                                         AS PurchaseDate,
        COALESCE(i.PrintName, 'Unknown Item')                                                        AS ItemName,
        COALESCE(i.BarCode, '')                                                                       AS BarCode,
        COALESCE(s.Name, 'No Supplier')                                                              AS SupplierName,
        audit.AdjustmentQuantity                                                                     AS QuantityPurchased,
        audit.UnitType                                                                               AS UnitType,
        COALESCE(
            (
                SELECT a.BuyingPrice FROM tbl_item_price_audits a
                WHERE a.ItemUuid = i.Uuid AND a.ChangedAt <= audit.UpdatedAt
                ORDER BY a.ChangedAt DESC, a.Id DESC LIMIT 1
            ),
            ip.BuyingPrice,
            0.00
        )                                                                                            AS UnitCost,
        COALESCE(ROUND(
            COALESCE(
                (
                    SELECT a.BuyingPrice FROM tbl_item_price_audits a
                    WHERE a.ItemUuid = i.Uuid AND a.ChangedAt <= audit.UpdatedAt
                    ORDER BY a.ChangedAt DESC, a.Id DESC LIMIT 1
                ),
                ip.BuyingPrice,
                0.00
            ) * audit.AdjustmentQuantity, 2), 0.00
        )                                                                                            AS TotalValue,
        COALESCE(NULLIF(TRIM(CONCAT(u.FirstName, ' ', IFNULL(u.LastName, ''))), ''), u.UserName, 'System') AS RecordedBy,
        COALESCE(audit.Reason, '-')                                                                  AS Reason,
        COALESCE(audit.Comment, '-')                                                                 AS Comment
    FROM tbl_inventory_adjust_audits audit
    JOIN tbl_inventories inv ON audit.InventoryUuid = inv.Uuid
    JOIN tbl_items i ON inv.ItemUuid = i.Uuid AND i.IsActive = 1
    LEFT JOIN tbl_item_prices ip ON i.Uuid = ip.ItemUuid
    LEFT JOIN tbl_item_suppliers isu ON i.Id = isu.ItemsId AND i.SubId = isu.ItemsSubId
    LEFT JOIN tbl_suppliers s ON isu.SuppliersId = s.Id AND s.IsActive = 1
    LEFT JOIN tbl_users u ON audit.UpdatedBy = u.Uuid
    WHERE audit.Increase = 1
      AND (p_StartDate IS NULL OR DATE(audit.UpdatedAt) >= DATE(p_StartDate))
      AND (p_EndDate IS NULL OR DATE(audit.UpdatedAt) <= DATE(p_EndDate))
    ORDER BY audit.UpdatedAt DESC;
END //
DELIMITER ;
