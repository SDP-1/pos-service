DROP PROCEDURE IF EXISTS `sp_report_cashier_performance`;
DELIMITER //
CREATE PROCEDURE `sp_report_cashier_performance`(IN p_StartDate DATETIME, IN p_EndDate DATETIME)
BEGIN
    SELECT 
        u.Id AS CashierId,
        COALESCE(NULLIF(TRIM(CONCAT(u.FirstName, ' ', IFNULL(u.LastName, ''))), ''), u.UserName, 'System') AS FullName,
        COALESCE(r.Name, 'User') AS RoleName,
        COUNT(o.Id) AS TotalOrdersProcessed,
        COALESCE(SUM(o.NetAmount), 0.00) AS TotalSalesAmount,
        COALESCE(SUM(o.TotalDiscount), 0.00) AS TotalDiscountsGiven,
        COALESCE(ROUND(AVG(o.NetAmount), 2), 0.00) AS AverageOrderValue
    FROM tbl_users u
    JOIN tbl_orders o ON u.Id = o.CashierId AND o.IsActive = 1
    LEFT JOIN tbl_roles r ON u.RoleId = r.Id
    WHERE u.IsActive = 1
      AND (p_StartDate IS NULL OR DATE(o.CreatedAt) >= DATE(p_StartDate))
      AND (p_EndDate IS NULL OR DATE(o.CreatedAt) <= DATE(p_EndDate))
    GROUP BY u.Id, u.FirstName, u.LastName, u.UserName, r.Name
    ORDER BY TotalSalesAmount DESC;
END //
DELIMITER ;

-- Created on 2026-07-31
-- Applied on dev 2026-07-31
-- Applied on prod 2026-08-23