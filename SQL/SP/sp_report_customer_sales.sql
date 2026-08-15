DROP PROCEDURE IF EXISTS `sp_report_customer_sales`;
DELIMITER //
CREATE PROCEDURE `sp_report_customer_sales`(IN p_StartDate DATETIME, IN p_EndDate DATETIME)
BEGIN
    SELECT 
        c.Id AS CustomerId,
        COALESCE(NULLIF(TRIM(CONCAT(c.FirstName, ' ', IFNULL(c.LastName, ''))), ''), CONCAT('Customer #', c.Id)) AS CustomerName,
        COALESCE(c.PhoneNumber, '-') AS PhoneNumber,
        COUNT(o.Id) AS OrderCount,
        COALESCE(SUM(o.NetAmount), 0.00) AS TotalSpent,
        COALESCE(SUM(CASE WHEN o.MainStatus = 'Loan' THEN ABS(o.Balance) ELSE 0.00 END), 0.00) AS OutstandingAmount
    FROM tbl_customers c
    JOIN tbl_orders o ON c.Id = o.CustomerId AND o.IsActive = 1
    WHERE c.IsActive = 1
      AND (p_StartDate IS NULL OR DATE(o.CreatedAt) >= DATE(p_StartDate))
      AND (p_EndDate IS NULL OR DATE(o.CreatedAt) <= DATE(p_EndDate))
    GROUP BY c.Id, c.FirstName, c.LastName, c.PhoneNumber
    ORDER BY TotalSpent DESC;
END //
DELIMITER ;

