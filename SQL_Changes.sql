
-- User table

ALTER TABLE `pos-system`.`users` 
CHANGE COLUMN `ProfileImageUrl` `ProfileImage` LONGTEXT CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NULL DEFAULT NULL ;

ALTER TABLE `pos-system`.`users` 
CHANGE COLUMN `NIC` `NIC` VARCHAR(12) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NULL DEFAULT NULL ;

-- order table
-- fix : remove system user , he created orders are going to remove

ALTER TABLE `pos-system`.`orders` 
DROP FOREIGN KEY `FK_Orders_Users_CashierId`;
ALTER TABLE `pos-system`.`orders` 
CHANGE COLUMN `CashierId` `CashierId` INT NULL ;
ALTER TABLE `pos-system`.`orders` 
ADD CONSTRAINT `FK_Orders_Users_CashierId`
  FOREIGN KEY (`CashierId`)
  REFERENCES `pos-system`.`users` (`Id`)
  ON DELETE SET NULL;

-- permission table
 -- chnaege permission  name and description

    UPDATE `pos-system`.`permissions` SET `PermissionType` = 'USER_ACTIVE_STATUS_CHANGE', `Description` = 'Can change user active status users' WHERE (`Id` = '303');

-- order items
-- add new 2 cloums in to table
-- for retun items and description

    ALTER TABLE `pos-system`.`orderitems` 
    ADD COLUMN `IsReturnItem` TINYINT(1) NOT NULL DEFAULT '0' AFTER `LineTotal`,
    ADD COLUMN `Description` VARCHAR(500) NULL DEFAULT NULL AFTER `IsReturnItem`;

    ALTER TABLE `pos-system`.`orderitems` 
    ADD COLUMN `ReturnedOrderItemUuid` VARCHAR(36) NULL DEFAULT NULL AFTER `IsReturnItem`;

-- view
----

        CREATE OR REPLACE VIEW View_ReturnedItemsSummary AS
        SELECT 
            -- Order Identifiers
            o.Id AS OrderId,
            o.OrderNumber,
            o.Uuid AS OrderUuid,
    
            -- Item Details
            original_item.PrintName,
            original_item.Uuid AS ReturnedOrderItemUuid,
    
            -- Quantity Calculations
            original_item.Quantity AS OriginalPurchasedQty,
            SUM(return_item.Quantity) AS TotalReturnedQty,
            (original_item.Quantity - SUM(return_item.Quantity)) AS RemainingQty,
    
            -- Financials
            original_item.PriceAtSale,
            CAST(SUM(return_item.Quantity) * original_item.PriceAtSale AS DECIMAL(18,2)) AS TotalRefundAmountValue

        FROM orderitems return_item
        -- Linking the return row to the original sale row
        INNER JOIN orderitems original_item 
            ON return_item.ReturnedOrderItemUuid = original_item.Uuid
        -- Linking to the order header
        INNER JOIN orders o 
            ON original_item.OrderId = o.Id

        WHERE return_item.IsReturnItem = 1
        GROUP BY 
            o.Id, 
            o.OrderNumber, 
            o.Uuid, 
            original_item.Uuid, 
            original_item.PrintName, 
            original_item.Quantity, 
            original_item.PriceAtSale;

-- contact
--
ALTER TABLE `pos-system`.`customers` 
ADD UNIQUE INDEX `Email_UNIQUE` (`Email` ASC) VISIBLE;
;









