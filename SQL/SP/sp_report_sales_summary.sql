DROP PROCEDURE IF EXISTS `sp_report_sales_summary`;
DELIMITER //
CREATE PROCEDURE `sp_report_sales_summary`(IN p_StartDate DATETIME, IN p_EndDate DATETIME)
BEGIN
    SELECT 
        COUNT(o.Id) AS TotalOrders,
        COALESCE(SUM(o.ItemCount), 0) AS TotalItemsSold,
        COALESCE(SUM(o.GrossAmount), 0.00) AS GrossRevenue,
        COALESCE(SUM(o.TotalDiscount), 0.00) AS TotalDiscount,
        COALESCE(SUM(o.NetAmount), 0.00) AS NetRevenue,
        COALESCE(SUM(o.TotalCost), 0.00) AS TotalCost,
        COALESCE(SUM(o.NetAmount - o.TotalCost), 0.00) AS NetProfit,
        COALESCE(ROUND(AVG(o.NetAmount), 2), 0.00) AS AverageOrderValue
    FROM tbl_orders o
    WHERE o.IsActive = 1
      AND (p_StartDate IS NULL OR DATE(o.CreatedAt) >= DATE(p_StartDate))
      AND (p_EndDate IS NULL OR DATE(o.CreatedAt) <= DATE(p_EndDate));
END //
DELIMITER ;
