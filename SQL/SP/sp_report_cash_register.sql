DROP PROCEDURE IF EXISTS `sp_report_cash_register`;
DELIMITER //
CREATE PROCEDURE `sp_report_cash_register`(IN p_StartDate DATETIME, IN p_EndDate DATETIME)
BEGIN
    SELECT 
        o.PaymentMethod,
        COUNT(o.Id) AS OrderCount,
        COALESCE(SUM(o.AmountPaid), 0.00) AS TotalPaid,
        COALESCE(SUM(o.Balance), 0.00) AS TotalBalanceOutstanding,
        COALESCE(SUM(o.NetAmount), 0.00) AS TotalNetRevenue
    FROM tbl_orders o
    WHERE o.IsActive = 1
      AND (p_StartDate IS NULL OR DATE(o.CreatedAt) >= DATE(p_StartDate))
      AND (p_EndDate IS NULL OR DATE(o.CreatedAt) <= DATE(p_EndDate))
    GROUP BY o.PaymentMethod;
END //
DELIMITER ;
