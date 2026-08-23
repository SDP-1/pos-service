DROP PROCEDURE IF EXISTS `sp_report_profit_loss`;
DELIMITER //
CREATE PROCEDURE `sp_report_profit_loss`(IN p_StartDate DATETIME, IN p_EndDate DATETIME)
BEGIN
    SELECT 
        COALESCE(SUM(o.NetAmount), 0.00) AS TotalSalesRevenue,
        COALESCE(SUM(o.TotalCost), 0.00) AS CostOfGoodsSold,
        COALESCE(SUM(o.NetAmount - o.TotalCost), 0.00) AS GrossProfit,
        COALESCE(SUM(o.TotalDiscount), 0.00) AS TotalDiscountsGiven,
        COALESCE(SUM(o.NetAmount - o.TotalCost), 0.00) AS NetProfit,
        CASE WHEN SUM(o.NetAmount) > 0 THEN ((SUM(o.NetAmount - o.TotalCost) / SUM(o.NetAmount)) * 100) ELSE 0.00 END AS MarginPercentage
    FROM tbl_orders o
    WHERE o.IsActive = 1
      AND (p_StartDate IS NULL OR DATE(o.CreatedAt) >= DATE(p_StartDate))
      AND (p_EndDate IS NULL OR DATE(o.CreatedAt) <= DATE(p_EndDate));
END //
DELIMITER ;

-- Created on 2026-07-31
-- Applied on dev 2026-07-31
-- Applied on prod 2026-08-23