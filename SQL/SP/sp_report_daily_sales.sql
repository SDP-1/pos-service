DROP PROCEDURE IF EXISTS `sp_report_daily_sales`;
DELIMITER //
CREATE PROCEDURE `sp_report_daily_sales`(IN p_StartDate DATETIME, IN p_EndDate DATETIME)
BEGIN
    SELECT 
        DATE(o.CreatedAt) AS SalesDate,
        COUNT(o.Id) AS TotalOrders,
        COALESCE(SUM(o.GrossAmount), 0.00) AS GrossRevenue,
        COALESCE(SUM(o.TotalDiscount), 0.00) AS TotalDiscount,
        COALESCE(SUM(o.NetAmount), 0.00) AS NetRevenue,
        COALESCE(SUM(o.TotalCost), 0.00) AS TotalCost,
        COALESCE(SUM(o.NetAmount - o.TotalCost), 0.00) AS GrossProfit
    FROM tbl_orders o
    WHERE o.IsActive = 1
      AND (p_StartDate IS NULL OR DATE(o.CreatedAt) >= DATE(p_StartDate))
      AND (p_EndDate IS NULL OR DATE(o.CreatedAt) <= DATE(p_EndDate))
    GROUP BY DATE(o.CreatedAt)
    ORDER BY SalesDate DESC;
END //
DELIMITER ;
