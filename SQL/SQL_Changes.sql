-- need to apply in prod




/*
-- appilied

ALTER TABLE `pos-system`.`items` 
ADD INDEX `IN_Main_Id` (`Id` ASC) VISIBLE,
ADD INDEX `IN_Barcode` (`BarCode` ASC) INVISIBLE;
ALTER TABLE `pos-system`.`items` ALTER INDEX `FK_Items_Users_UpdatedBy` INVISIBLE;

-- name chnaged view_returneditemssummary to view_returned_items_summary
DROP VIEW IF EXISTS `pos-system`.`view_returneditemssummary` ;
USE `pos-system`;
CREATE  OR REPLACE 
    ALGORITHM = UNDEFINED 
    DEFINER = `root`@`localhost` 
    SQL SECURITY DEFINER
VIEW `view_returned_items_summary` AS
    SELECT 
        `o`.`Id` AS `OrderId`,
        `o`.`OrderNumber` AS `OrderNumber`,
        `o`.`Uuid` AS `OrderUuid`,
        `original_item`.`PrintName` AS `PrintName`,
        `original_item`.`Uuid` AS `ReturnedOrderItemUuid`,
        `original_item`.`Quantity` AS `OriginalPurchasedQty`,
        SUM(`return_item`.`Quantity`) AS `TotalReturnedQty`,
        (`original_item`.`Quantity` - SUM(`return_item`.`Quantity`)) AS `RemainingQty`,
        `original_item`.`PriceAtSale` AS `PriceAtSale`,
        CAST((SUM(`return_item`.`Quantity`) * `original_item`.`PriceAtSale`)
            AS DECIMAL (18 , 2 )) AS `TotalRefundAmountValue`
    FROM
        ((`orderitems` `return_item`
        JOIN `orderitems` `original_item` ON ((`return_item`.`ReturnedOrderItemUuid` = `original_item`.`Uuid`)))
        JOIN `orders` `o` ON ((`original_item`.`OrderId` = `o`.`Id`)))
    WHERE
        (`return_item`.`IsReturnItem` = 1)
    GROUP BY `o`.`Id` , `o`.`OrderNumber` , `o`.`Uuid` , `original_item`.`Uuid` , `original_item`.`PrintName` , `original_item`.`Quantity` , `original_item`.`PriceAtSale`;

    -- file
    InventoryAudit_SQL.sql

    -- SQL template table

    CREATE TABLE `sqltemplates` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `TemplateName` varchar(255) NOT NULL,
  `Description` varchar(1000) DEFAULT NULL,
  `SqlQuery` longtext NOT NULL,
  `PlaceholdersJson` longtext,
  `SelectValuesJson` longtext,
  `Uuid` varchar(36) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `CreatedBy` varchar(36) DEFAULT NULL,
  `UpdatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  `UpdatedBy` varchar(36) DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Uuid` (`Uuid`),
  UNIQUE KEY `TemplateName` (`TemplateName`),
  KEY `CreatedBy` (`CreatedBy`),
  KEY `UpdatedBy` (`UpdatedBy`),
  CONSTRAINT `sqltemplates_ibfk_1` FOREIGN KEY (`CreatedBy`) REFERENCES `users` (`Uuid`) ON DELETE SET NULL,
  CONSTRAINT `sqltemplates_ibfk_2` FOREIGN KEY (`UpdatedBy`) REFERENCES `users` (`Uuid`) ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=14 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci

-- reporttemplates

CREATE TABLE `reporttemplates` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ReportName` varchar(255) NOT NULL,
  `Description` varchar(1000) DEFAULT NULL,
  `HtmlContent` longtext NOT NULL,
  `ParametersJson` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `SqlPlaceholderMappingsJson` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Uuid` varchar(36) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  `CreatedBy` varchar(36) DEFAULT NULL,
  `UpdatedBy` varchar(36) DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Uuid` (`Uuid`),
  UNIQUE KEY `ReportName` (`ReportName`),
  KEY `reporttemplates_ibfk_1` (`CreatedBy`),
  KEY `reporttemplates_ibfk_2` (`UpdatedBy`),
  CONSTRAINT `reporttemplates_ibfk_1` FOREIGN KEY (`CreatedBy`) REFERENCES `users` (`Uuid`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `reporttemplates_ibfk_2` FOREIGN KEY (`UpdatedBy`) REFERENCES `users` (`Uuid`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci

-- reporttemplatesqltemplates

CREATE TABLE `reporttemplatesqltemplates` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ReportTemplateId` int NOT NULL,
  `SqlTemplateId` int NOT NULL,
  `Uuid` varchar(36) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  `CreatedBy` varchar(36) DEFAULT NULL,
  `UpdatedBy` varchar(36) DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `AK_ReportTemplateSqlTemplates_Uuid` (`Uuid`),
  KEY `IX_ReportTemplateSqlTemplates_ReportTemplateId` (`ReportTemplateId`),
  KEY `IX_ReportTemplateSqlTemplates_SqlTemplateId` (`SqlTemplateId`),
  KEY `IX_ReportTemplateSqlTemplates_CreatedBy` (`CreatedBy`),
  KEY `IX_ReportTemplateSqlTemplates_UpdatedBy` (`UpdatedBy`),
  CONSTRAINT `FK_ReportTemplateSqlTemplates_ReportTemplates_ReportTemplateId` FOREIGN KEY (`ReportTemplateId`) REFERENCES `reporttemplates` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_ReportTemplateSqlTemplates_Users_CreatedBy` FOREIGN KEY (`CreatedBy`) REFERENCES `users` (`Uuid`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `FK_ReportTemplateSqlTemplates_Users_UpdatedBy` FOREIGN KEY (`UpdatedBy`) REFERENCES `users` (`Uuid`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `FK_SqlTemplate_Id` FOREIGN KEY (`SqlTemplateId`) REFERENCES `sqltemplates` (`Id`) ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=127 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci


CREATE TABLE `inventories` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ItemUuid` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `StockQuantity` decimal(18,3) NOT NULL,
  `AllowsDecimalQuantities` tinyint(1) NOT NULL,
  `UnitType` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Uuid` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  `CreatedBy` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `UpdatedBy` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `AK_Inventories_Uuid` (`Uuid`),
  UNIQUE KEY `IX_Inventories_ItemUuid` (`ItemUuid`),
  KEY `IX_Inventories_CreatedBy` (`CreatedBy`),
  KEY `IX_Inventories_UpdatedBy` (`UpdatedBy`),
  CONSTRAINT `FK_Inventories_Items_ItemUuid` FOREIGN KEY (`ItemUuid`) REFERENCES `items` (`Uuid`) ON DELETE CASCADE,
  CONSTRAINT `FK_Inventories_Users_CreatedBy` FOREIGN KEY (`CreatedBy`) REFERENCES `users` (`Uuid`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `FK_Inventories_Users_UpdatedBy` FOREIGN KEY (`UpdatedBy`) REFERENCES `users` (`Uuid`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=1080 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci

CREATE TABLE `inventoryunits` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `InventoryId` int NOT NULL,
  `ParentUnitType` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `UnitType` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `QuantityPerParent` decimal(18,3) NOT NULL DEFAULT '0.000',
  `QuantityInBaseUnits` decimal(18,3) NOT NULL,
  `Uuid` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  `CreatedBy` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `UpdatedBy` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `AK_InventoryUnits_Uuid` (`Uuid`),
  KEY `IX_InventoryUnits_InventoryId` (`InventoryId`),
  KEY `IX_InventoryUnits_CreatedBy` (`CreatedBy`),
  KEY `IX_InventoryUnits_UpdatedBy` (`UpdatedBy`),
  CONSTRAINT `FK_InventoryUnits_Inventories_InventoryId` FOREIGN KEY (`InventoryId`) REFERENCES `inventories` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_InventoryUnits_Users_CreatedBy` FOREIGN KEY (`CreatedBy`) REFERENCES `users` (`Uuid`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `FK_InventoryUnits_Users_UpdatedBy` FOREIGN KEY (`UpdatedBy`) REFERENCES `users` (`Uuid`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=95 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci

INSERT INTO inventories (
    ItemUuid, 
    StockQuantity, 
    AllowsDecimalQuantities, 
    UnitType, 
    Uuid, 
    CreatedBy, 
    UpdatedBy, 
    IsActive
)
SELECT 
    i.Uuid,                      
    0.000,                       
    i.AllowsDecimalQuantities,   -- FIX HERE
    'Packet',                    
    UUID(),                      
    NULL,                        
    NULL,                        
    1                            
FROM items i
LEFT JOIN inventories inv 
    ON i.Uuid = inv.ItemUuid
WHERE inv.ItemUuid IS NULL;

------------------------------------

-- BackupHistories
ALTER TABLE BackupHistories DROP FOREIGN KEY FK_BackupHistories_Users_CreatedBy;
ALTER TABLE BackupHistories DROP FOREIGN KEY FK_BackupHistories_Users_UpdatedBy;

-- BackupLocations
ALTER TABLE BackupLocations DROP FOREIGN KEY FK_BackupLocations_Users_CreatedBy;
ALTER TABLE BackupLocations DROP FOREIGN KEY FK_BackupLocations_Users_UpdatedBy;

-- Contacts
ALTER TABLE Contacts DROP FOREIGN KEY FK_Contacts_Users_CreatedBy;
ALTER TABLE Contacts DROP FOREIGN KEY FK_Contacts_Users_UpdatedBy;

-- Customers
ALTER TABLE Customers DROP FOREIGN KEY FK_Customers_Users_CreatedBy;
ALTER TABLE Customers DROP FOREIGN KEY FK_Customers_Users_UpdatedBy;

-- Inventories
ALTER TABLE Inventories DROP FOREIGN KEY FK_Inventories_Users_CreatedBy;
ALTER TABLE Inventories DROP FOREIGN KEY FK_Inventories_Users_UpdatedBy;

-- InventoryUnits
ALTER TABLE InventoryUnits DROP FOREIGN KEY FK_InventoryUnits_Users_CreatedBy;
ALTER TABLE InventoryUnits DROP FOREIGN KEY FK_InventoryUnits_Users_UpdatedBy;

-- ItemExpiries
ALTER TABLE ItemExpiries DROP FOREIGN KEY FK_ItemExpiries_Users_CreatedBy;
ALTER TABLE ItemExpiries DROP FOREIGN KEY FK_ItemExpiries_Users_UpdatedBy;

-- ItemPrices
ALTER TABLE ItemPrices DROP FOREIGN KEY FK_ItemPrices_Users_CreatedBy;
ALTER TABLE ItemPrices DROP FOREIGN KEY FK_ItemPrices_Users_UpdatedBy;

-- Items
ALTER TABLE Items DROP FOREIGN KEY FK_Items_Users_CreatedBy;
ALTER TABLE Items DROP FOREIGN KEY FK_Items_Users_UpdatedBy;

-- ItemSuppliers
ALTER TABLE ItemSuppliers DROP FOREIGN KEY FK_ItemSuppliers_Users_CreatedBy;
ALTER TABLE ItemSuppliers DROP FOREIGN KEY FK_ItemSuppliers_Users_UpdatedBy;

-- Orders
ALTER TABLE Orders DROP FOREIGN KEY FK_Orders_Users_CreatedBy;
ALTER TABLE Orders DROP FOREIGN KEY FK_Orders_Users_UpdatedBy;

-- OrderItems
ALTER TABLE OrderItems DROP FOREIGN KEY FK_OrderItems_Users_CreatedBy;
ALTER TABLE OrderItems DROP FOREIGN KEY FK_OrderItems_Users_UpdatedBy;

-- RolePermissions
ALTER TABLE RolePermissions DROP FOREIGN KEY FK_RolePermissions_Users_CreatedBy;
ALTER TABLE RolePermissions DROP FOREIGN KEY FK_RolePermissions_Users_UpdatedBy;

-- Roles
ALTER TABLE Roles DROP FOREIGN KEY FK_Roles_Users_CreatedBy;
ALTER TABLE Roles DROP FOREIGN KEY FK_Roles_Users_UpdatedBy;

-- Settings
ALTER TABLE Settings DROP FOREIGN KEY FK_Settings_Users_CreatedBy;
ALTER TABLE Settings DROP FOREIGN KEY FK_Settings_Users_UpdatedBy;

-- Shops
ALTER TABLE Shops DROP FOREIGN KEY FK_Shops_Users_CreatedBy;
ALTER TABLE Shops DROP FOREIGN KEY FK_Shops_Users_UpdatedBy;

-- Suppliers
ALTER TABLE Suppliers DROP FOREIGN KEY FK_Suppliers_Users_CreatedBy;
ALTER TABLE Suppliers DROP FOREIGN KEY FK_Suppliers_Users_UpdatedBy;

-- Users self reference
ALTER TABLE Users DROP FOREIGN KEY FK_Users_Users_CreatedBy;
ALTER TABLE Users DROP FOREIGN KEY FK_Users_Users_UpdatedBy;

-- BackupHistories
ALTER TABLE BackupHistories
ADD CONSTRAINT FK_BackupHistories_Users_CreatedBy
FOREIGN KEY (CreatedBy) REFERENCES Users(Uuid)
ON DELETE SET NULL ON UPDATE CASCADE;

ALTER TABLE BackupHistories
ADD CONSTRAINT FK_BackupHistories_Users_UpdatedBy
FOREIGN KEY (UpdatedBy) REFERENCES Users(Uuid)
ON DELETE SET NULL ON UPDATE CASCADE;

-- BackupLocations
ALTER TABLE BackupLocations
ADD CONSTRAINT FK_BackupLocations_Users_CreatedBy
FOREIGN KEY (CreatedBy) REFERENCES Users(Uuid)
ON DELETE SET NULL ON UPDATE CASCADE;

ALTER TABLE BackupLocations
ADD CONSTRAINT FK_BackupLocations_Users_UpdatedBy
FOREIGN KEY (UpdatedBy) REFERENCES Users(Uuid)
ON DELETE SET NULL ON UPDATE CASCADE;

-- Contacts
ALTER TABLE Contacts
ADD CONSTRAINT FK_Contacts_Users_CreatedBy
FOREIGN KEY (CreatedBy) REFERENCES Users(Uuid)
ON DELETE SET NULL ON UPDATE CASCADE;

ALTER TABLE Contacts
ADD CONSTRAINT FK_Contacts_Users_UpdatedBy
FOREIGN KEY (UpdatedBy) REFERENCES Users(Uuid)
ON DELETE SET NULL ON UPDATE CASCADE;

-- Customers
ALTER TABLE Customers
ADD CONSTRAINT FK_Customers_Users_CreatedBy
FOREIGN KEY (CreatedBy) REFERENCES Users(Uuid)
ON DELETE SET NULL ON UPDATE CASCADE;

ALTER TABLE Customers
ADD CONSTRAINT FK_Customers_Users_UpdatedBy
FOREIGN KEY (UpdatedBy) REFERENCES Users(Uuid)
ON DELETE SET NULL ON UPDATE CASCADE;

-- Inventories
ALTER TABLE Inventories
ADD CONSTRAINT FK_Inventories_Users_CreatedBy
FOREIGN KEY (CreatedBy) REFERENCES Users(Uuid)
ON DELETE SET NULL ON UPDATE CASCADE;

ALTER TABLE Inventories
ADD CONSTRAINT FK_Inventories_Users_UpdatedBy
FOREIGN KEY (UpdatedBy) REFERENCES Users(Uuid)
ON DELETE SET NULL ON UPDATE CASCADE;

-- InventoryUnits
ALTER TABLE InventoryUnits
ADD CONSTRAINT FK_InventoryUnits_Users_CreatedBy
FOREIGN KEY (CreatedBy) REFERENCES Users(Uuid)
ON DELETE SET NULL ON UPDATE CASCADE;

ALTER TABLE InventoryUnits
ADD CONSTRAINT FK_InventoryUnits_Users_UpdatedBy
FOREIGN KEY (UpdatedBy) REFERENCES Users(Uuid)
ON DELETE SET NULL ON UPDATE CASCADE;

-- ItemExpiries
ALTER TABLE ItemExpiries
ADD CONSTRAINT FK_ItemExpiries_Users_CreatedBy
FOREIGN KEY (CreatedBy) REFERENCES Users(Uuid)
ON DELETE SET NULL ON UPDATE CASCADE;

ALTER TABLE ItemExpiries
ADD CONSTRAINT FK_ItemExpiries_Users_UpdatedBy
FOREIGN KEY (UpdatedBy) REFERENCES Users(Uuid)
ON DELETE SET NULL ON UPDATE CASCADE;

-- ItemPrices
ALTER TABLE ItemPrices
ADD CONSTRAINT FK_ItemPrices_Users_CreatedBy
FOREIGN KEY (CreatedBy) REFERENCES Users(Uuid)
ON DELETE SET NULL ON UPDATE CASCADE;

ALTER TABLE ItemPrices
ADD CONSTRAINT FK_ItemPrices_Users_UpdatedBy
FOREIGN KEY (UpdatedBy) REFERENCES Users(Uuid)
ON DELETE SET NULL ON UPDATE CASCADE;

-- Items
ALTER TABLE Items
ADD CONSTRAINT FK_Items_Users_CreatedBy
FOREIGN KEY (CreatedBy) REFERENCES Users(Uuid)
ON DELETE SET NULL ON UPDATE CASCADE;

ALTER TABLE Items
ADD CONSTRAINT FK_Items_Users_UpdatedBy
FOREIGN KEY (UpdatedBy) REFERENCES Users(Uuid)
ON DELETE SET NULL ON UPDATE CASCADE;

-- ItemSuppliers
ALTER TABLE ItemSuppliers
ADD CONSTRAINT FK_ItemSuppliers_Users_CreatedBy
FOREIGN KEY (CreatedBy) REFERENCES Users(Uuid)
ON DELETE SET NULL ON UPDATE CASCADE;

ALTER TABLE ItemSuppliers
ADD CONSTRAINT FK_ItemSuppliers_Users_UpdatedBy
FOREIGN KEY (UpdatedBy) REFERENCES Users(Uuid)
ON DELETE SET NULL ON UPDATE CASCADE;

-- Orders
ALTER TABLE Orders
ADD CONSTRAINT FK_Orders_Users_CreatedBy
FOREIGN KEY (CreatedBy) REFERENCES Users(Uuid)
ON DELETE SET NULL ON UPDATE CASCADE;

ALTER TABLE Orders
ADD CONSTRAINT FK_Orders_Users_UpdatedBy
FOREIGN KEY (UpdatedBy) REFERENCES Users(Uuid)
ON DELETE SET NULL ON UPDATE CASCADE;

-- OrderItems
ALTER TABLE OrderItems
ADD CONSTRAINT FK_OrderItems_Users_CreatedBy
FOREIGN KEY (CreatedBy) REFERENCES Users(Uuid)
ON DELETE SET NULL ON UPDATE CASCADE;

ALTER TABLE OrderItems
ADD CONSTRAINT FK_OrderItems_Users_UpdatedBy
FOREIGN KEY (UpdatedBy) REFERENCES Users(Uuid)
ON DELETE SET NULL ON UPDATE CASCADE;

-- Users (self reference)
ALTER TABLE Users
ADD CONSTRAINT FK_Users_Users_CreatedBy
FOREIGN KEY (CreatedBy) REFERENCES Users(Uuid)
ON DELETE SET NULL ON UPDATE CASCADE;

ALTER TABLE Users
ADD CONSTRAINT FK_Users_Users_UpdatedBy
FOREIGN KEY (UpdatedBy) REFERENCES Users(Uuid)
ON DELETE SET NULL ON UPDATE CASCADE;


-- IF needed use this 

SET SQL_SAFE_UPDATES = 0;
UPDATE `pos-system`.`(table name)`
SET CreatedBy = NULL,
    UpdatedBy = NULL;
SET SQL_SAFE_UPDATES = 1;

--------------------------

-- CREATE TABLE `shops` (
--   `Id` int NOT NULL AUTO_INCREMENT,
--   `Name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
--   `Address` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
--   `PhoneNumber` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
--   `Email` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
--   `Logo` mediumblob,
--   `Uuid` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
--   `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
--   `UpdatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
--   `CreatedBy` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
--   `UpdatedBy` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
--   `IsActive` tinyint(1) NOT NULL DEFAULT '1',
--   PRIMARY KEY (`Id`),
--   UNIQUE KEY `AK_Shops_Uuid` (`Uuid`),
--   UNIQUE KEY `IX_Shops_Name` (`Name`)
-- ) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci

-- -- chnage user table profileimage type as LONGTEXT to MEDIUMBLOB

-- ALTER TABLE `pos-system`.`users` 
-- CHANGE COLUMN `ProfileImage` `ProfileImage` MEDIUMBLOB NULL DEFAULT NULL ;

--------------
ALTER TABLE `pos-system`.`settings` 
CHANGE COLUMN `Description` `Description` VARCHAR(600) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NULL DEFAULT NULL ;
--------

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









