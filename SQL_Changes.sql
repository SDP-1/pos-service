-- need to apply

ALTER TABLE `pos-system`.`settings` 
CHANGE COLUMN `Description` `Description` VARCHAR(600) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NULL DEFAULT NULL ;





/*
-- appilied

ALTER TABLE `pos-system`.`settings` 
ADD COLUMN `SettingName` VARCHAR(100) NOT NULL AFTER `SettingKey`;

ALTER TABLE `pos-system`.`permissions` 
CHANGE COLUMN `PermissionType` `PermissionType` VARCHAR(50) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NOT NULL ;

ALTER TABLE `pos-system`.`permissions` 
CHANGE COLUMN `PermissionCatagory` `PermissionCatagory` VARCHAR(50) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NOT NULL ;

ALTER TABLE `pos-system`.`settings` 
CHANGE COLUMN `SettingKey` `SettingKey` VARCHAR(50) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NOT NULL ;


-- 1. Add the new columns as VARCHAR to match your data
ALTER TABLE `orders` 
ADD COLUMN `MainStatus` VARCHAR(50) NOT NULL AFTER `Status`,
ADD COLUMN `SubStatus` VARCHAR(50) NULL DEFAULT NULL AFTER `MainStatus`;

SET SQL_SAFE_UPDATES = 0;
UPDATE `orders` SET `MainStatus` = `Status`;

UPDATE `orders` SET `MainStatus` = 'Paid', `SubStatus` = 'Return' WHERE `Status` = 'Return';

SET SQL_SAFE_UPDATES = 1;

ALTER TABLE `orders` DROP COLUMN `Status`;

-- column updates

ALTER TABLE `pos-system`.`orders` 
CHANGE COLUMN `PaymentMethod` `PaymentMethod` VARCHAR(50) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NOT NULL ,
CHANGE COLUMN `SaleType` `SaleType` VARCHAR(50) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NOT NULL ;

-- column updates

ALTER TABLE `pos-system`.`orders` 
CHANGE COLUMN `Uuid` `Uuid` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NOT NULL ,
CHANGE COLUMN `CreatedBy` `CreatedBy` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NULL DEFAULT NULL ,
CHANGE COLUMN `UpdatedBy` `UpdatedBy` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NULL DEFAULT NULL ;

ALTER TABLE `pos-system`.`users` 
CHANGE COLUMN `UserName` `UserName` VARCHAR(100) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NOT NULL ,
CHANGE COLUMN `Uuid` `Uuid` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NOT NULL ,
CHANGE COLUMN `CreatedBy` `CreatedBy` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NULL DEFAULT NULL ,
CHANGE COLUMN `UpdatedBy` `UpdatedBy` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NULL DEFAULT NULL ;

ALTER TABLE `pos-system`.`suppliers` 
CHANGE COLUMN `Name` `Name` VARCHAR(100) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NOT NULL ,
CHANGE COLUMN `Uuid` `Uuid` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NOT NULL ,
CHANGE COLUMN `CreatedBy` `CreatedBy` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NULL DEFAULT NULL ,
CHANGE COLUMN `UpdatedBy` `UpdatedBy` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NULL DEFAULT NULL ;

ALTER TABLE `pos-system`.`settings` 
CHANGE COLUMN `Uuid` `Uuid` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NOT NULL ,
CHANGE COLUMN `CreatedBy` `CreatedBy` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NULL DEFAULT NULL ,
CHANGE COLUMN `UpdatedBy` `UpdatedBy` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NULL DEFAULT NULL ;

ALTER TABLE `pos-system`.`roles` 
CHANGE COLUMN `Uuid` `Uuid` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NOT NULL ,
CHANGE COLUMN `CreatedBy` `CreatedBy` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NULL DEFAULT NULL ,
CHANGE COLUMN `UpdatedBy` `UpdatedBy` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NULL DEFAULT NULL ;

ALTER TABLE `pos-system`.`rolepermissions` 
CHANGE COLUMN `Uuid` `Uuid` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NOT NULL ,
CHANGE COLUMN `CreatedBy` `CreatedBy` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NULL DEFAULT NULL ,
CHANGE COLUMN `UpdatedBy` `UpdatedBy` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NULL DEFAULT NULL ;

ALTER TABLE `pos-system`.`orderitems` 
DROP FOREIGN KEY `FK_OrderItems_Items_OriginalItemUuid`;
ALTER TABLE `pos-system`.`orderitems` 
CHANGE COLUMN `OriginalItemUuid` `OriginalItemUuid` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NULL DEFAULT NULL ,
CHANGE COLUMN `Uuid` `Uuid` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NOT NULL ,
CHANGE COLUMN `CreatedBy` `CreatedBy` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NULL DEFAULT NULL ,
CHANGE COLUMN `UpdatedBy` `UpdatedBy` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NULL DEFAULT NULL ;
ALTER TABLE `pos-system`.`orderitems` 
ADD CONSTRAINT `FK_OrderItems_Items_OriginalItemUuid`
  FOREIGN KEY (`OriginalItemUuid`)
  REFERENCES `pos-system`.`items` (`Uuid`)
  ON DELETE SET NULL;

  ALTER TABLE `pos-system`.`itemprices` 
CHANGE COLUMN `ItemUuid` `ItemUuid` VARCHAR(36) NOT NULL ,
CHANGE COLUMN `CreatedBy` `CreatedBy` VARCHAR(36) NULL DEFAULT NULL ,
CHANGE COLUMN `UpdatedBy` `UpdatedBy` VARCHAR(36) NULL DEFAULT NULL ,
CHANGE COLUMN `Uuid` `Uuid` VARCHAR(36) NOT NULL ;

ALTER TABLE `pos-system`.`itemexpiries` 
CHANGE COLUMN `ItemUuid` `ItemUuid` VARCHAR(36) NOT NULL ,
CHANGE COLUMN `Uuid` `Uuid` VARCHAR(36) NOT NULL ,
CHANGE COLUMN `CreatedBy` `CreatedBy` VARCHAR(36) NULL DEFAULT NULL ,
CHANGE COLUMN `UpdatedBy` `UpdatedBy` VARCHAR(36) NULL DEFAULT NULL ;

ALTER TABLE `pos-system`.`customers` 
CHANGE COLUMN `Uuid` `Uuid` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NOT NULL ,
CHANGE COLUMN `CreatedBy` `CreatedBy` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NULL DEFAULT NULL ,
CHANGE COLUMN `UpdatedBy` `UpdatedBy` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NULL DEFAULT NULL ;

ALTER TABLE `pos-system`.`contacts` 
CHANGE COLUMN `Uuid` `Uuid` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NOT NULL ,
CHANGE COLUMN `CreatedBy` `CreatedBy` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NULL DEFAULT NULL ,
CHANGE COLUMN `UpdatedBy` `UpdatedBy` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NULL DEFAULT NULL ;

ALTER TABLE `pos-system`.`backuplocations` 
CHANGE COLUMN `Uuid` `Uuid` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NOT NULL ,
CHANGE COLUMN `CreatedBy` `CreatedBy` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NULL DEFAULT NULL ,
CHANGE COLUMN `UpdatedBy` `UpdatedBy` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NULL DEFAULT NULL ;

ALTER TABLE `pos-system`.`backuphistories` 
CHANGE COLUMN `Uuid` `Uuid` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NOT NULL ,
CHANGE COLUMN `ScheduleUuid` `ScheduleUuid` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NOT NULL ,
CHANGE COLUMN `LocationUuid` `LocationUuid` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NOT NULL ,
CHANGE COLUMN `CreatedBy` `CreatedBy` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NULL DEFAULT NULL ,
CHANGE COLUMN `UpdatedBy` `UpdatedBy` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NULL DEFAULT NULL ;

--- create new table
CREATE TABLE `loansettlementlogs` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `OrderId` int NOT NULL,
  `PaymentDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `Description` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `AmountPaid` decimal(18,2) NOT NULL,
  `RemainingBalance` decimal(18,2) NOT NULL,
  `Status` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Uuid` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  `CreatedBy` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `UpdatedBy` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `AK_LoanSettlementLogs_Uuid` (`Uuid`),
  KEY `IX_LoanSettlementLogs_OrderId` (`OrderId`),
  CONSTRAINT `FK_LoanSettlementLogs_Orders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `orders` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci

---------------
-- STEP 1: Remove the constraint from the child table
ALTER TABLE `pos-system`.`orderitems` 
DROP FOREIGN KEY `FK_OrderItems_Items_OriginalItemUuid`;

-- STEP 2: Now you can safely modify the parent table (items)
ALTER TABLE `pos-system`.`items` 
MODIFY COLUMN `Uuid` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NOT NULL,
MODIFY COLUMN `CreatedBy` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NULL DEFAULT NULL,
MODIFY COLUMN `UpdatedBy` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NULL DEFAULT NULL;

-- STEP 3: Modify the child table (orderitems) to match the new length
ALTER TABLE `pos-system`.`orderitems` 
MODIFY COLUMN `OriginalItemUuid` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NULL DEFAULT NULL,
MODIFY COLUMN `Uuid` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NOT NULL,
MODIFY COLUMN `CreatedBy` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NULL DEFAULT NULL,
MODIFY COLUMN `UpdatedBy` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NULL DEFAULT NULL;

-- STEP 4: Restore the foreign key
ALTER TABLE `pos-system`.`orderitems` 
ADD CONSTRAINT `FK_OrderItems_Items_OriginalItemUuid`
  FOREIGN KEY (`OriginalItemUuid`)
  REFERENCES `pos-system`.`items` (`Uuid`)
  ON DELETE SET NULL;


  */




/*

-- below one applied.

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

*/









