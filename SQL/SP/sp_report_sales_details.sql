DROP PROCEDURE IF EXISTS `sp_report_sales_details`;
DELIMITER //
CREATE PROCEDURE `sp_report_sales_details`(IN p_StartDate DATETIME, IN p_EndDate DATETIME)
BEGIN
    SELECT 
        o.Id AS OrderId,
        o.OrderNumber,
        DATE(o.CreatedAt) AS OrderDate,
        COALESCE(NULLIF(TRIM(CONCAT(c.FirstName, ' ', IFNULL(c.LastName, ''))), ''), 'Walk-in Customer') AS CustomerName,
        COALESCE(NULLIF(TRIM(CONCAT(u.FirstName, ' ', IFNULL(u.LastName, ''))), ''), u.UserName, 'System') AS CashierName,
        o.PaymentMethod,
        o.SaleType,
        o.ItemCount,
        o.NetAmount,
        o.MainStatus
    FROM tbl_orders o
    LEFT JOIN tbl_customers c ON o.CustomerId = c.Id
    LEFT JOIN tbl_users u ON o.CashierId = u.Id
    WHERE o.IsActive = 1
      AND (p_StartDate IS NULL OR DATE(o.CreatedAt) >= DATE(p_StartDate))
      AND (p_EndDate IS NULL OR DATE(o.CreatedAt) <= DATE(p_EndDate))
    ORDER BY o.CreatedAt DESC;
END //
DELIMITER ;

-- Created on 2026-07-31
-- Applied on dev 2026-07-31
-- Applied on prod 2026-08-23